namespace Blueprint.Blue
{
    using XBlueprintBlue;
    using BlueprintBlue;
    using System.Linq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using BlueprintBlue.FuzzyLex;
    using AVXLib;

    public class QContext
    {
        public QSettings GlobalSettings { get; internal set; }
        public QSettings LocalSettings  { get; internal set; }
        public uint InvocationCount     { get; internal set; }

        public List<string> AsYaml()
        {
            return ICommand.YamlSerializer(this);
        }
        public XSettings AsMessage()
        {
            var xdomain = this.LocalSettings.Lexicon.Value == QLexicalDomain.QLexiconVal.BOTH ? XLexEnum.BOTH : this.LocalSettings.Lexicon.Value == QLexicalDomain.QLexiconVal.AV ? XLexEnum.AV : XLexEnum.AVX;
            var xdisplay = this.LocalSettings.Display.Value == QLexicalDisplay.QDisplayVal.AV ? XOutEnum.AV : XOutEnum.AVX;
            var xformat = XFmtEnum.JSON;
            if (this.LocalSettings.Format.Value != QFormat.QFormatVal.JSON)
            {
                if (this.LocalSettings.Format.Value != QFormat.QFormatVal.TEXT)
                    xformat = XFmtEnum.TEXT;
                else if (this.LocalSettings.Format.Value != QFormat.QFormatVal.HTML)
                    xformat = XFmtEnum.HTML;
                else if (this.LocalSettings.Format.Value != QFormat.QFormatVal.MD)
                    xformat = XFmtEnum.MD;
            }
            return new XSettings() { Similarity = this.LocalSettings.Similarity.ToString(), Span = this.LocalSettings.Span.Value, Format = xformat, Lexicon = xdomain, Display = xdisplay };
        }

        public string SessionAsString //pseudo-random-identifier (might not be needed)
        {
            get => Uniqueness.ToString() + ":" + Ticks.ToString();
        }
        public (UInt64 part1, UInt64 part2, Int32 ticks) Session //pseudo-random-identifier (might not be needed)
        {
            get
            {
                (UInt64 part1, UInt64 part2, Int32 ticks) result = (0, 0, this.Ticks);
                var bytes = this.Uniqueness.ToByteArray();
                for (int i = 0; i < bytes.Length; i++)
                {
                    if (i < 32)
                    {
                        result.part1 <<= 8;
                        result.part1 |= bytes[i];
                    }
                    else
                    {
                        result.part2 <<= 8;
                        result.part2 |= bytes[i];
                    }
                }
                return result;
            }
        }
        public UInt16[]?Fields { get; set; }

        public QStatement Statement { get; private set; }

        public string HistoryPath { get; private set; } // not used yet
        public string MacroPath { get; private set; }   // not used yet

        private Guid Uniqueness;
        private Int32 Ticks;

        static QContext()
        {
            BlueprintLex.Initialize(ObjectTable.AVXObjects);
        }
        private Dictionary<UInt32, QExpandableStatement> History = new();

        public QContext(QStatement statement)
        {
            this.Uniqueness = Guid.NewGuid();
            this.Ticks = Environment.TickCount;

            BlueprintBlue.FuzzyLex.BlueprintLex.Initialize(ObjectTable.AVXObjects);
            this.Statement = statement;
            this.InvocationCount = 0; // This can be updated when Create() is called on Implicit clauses
            this.GlobalSettings = statement.GlobalSettings;
            this.LocalSettings  = new QSettings(this.GlobalSettings);

            this.Fields  = null;    // Null means that no fields were provided; In Quelle, this is different than an empty array of fields
            this.HistoryPath = string.Empty;
            this.MacroPath = string.Empty;

            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AV-Bible");

            if (!string.IsNullOrEmpty(folder))
            {
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                this.HistoryPath = Path.Combine(folder, "History.Quelle").Replace("\\", "/");
                this.MacroPath = Path.Combine(folder, "Labels").Replace("\\", "/");
            }
            else
            {
                this.AddWarning("A session context cannot be established");
            }
            if (!ObjectTable.AVXObjects.Mem.valid)
            {
                this.AddError("Unable to load AVX Data. Without this library, other things will break");
            }
            this.ReadAllHistory();
        }
        public void AddError(string message)
        {
            this.Statement.AddError(message);
        }
        public QExpandableStatement? Expand(UInt32 seq)     // e.g. $1
        {
            return this.GetHistoryEntry(seq);
        }
        public QExpandableStatement? Expand(string label)   // e.g. $my-macro-def
        {
            return this.GetMacro(label);
        }
        public void AddWarning(string message)
        {
            this.Statement.AddWarning(message);
        }
        public void AddHistory(QExpandableStatement stmt)
        {
            if (!string.IsNullOrWhiteSpace(stmt.Statement))
            {
                QExpandableStatement? prev = null;
                UInt32 seq = 0;
                foreach (UInt32 i in from x in this.History.Keys orderby x descending select (UInt32) x)
                {
                    prev = this.History[i];
                    seq = (UInt32) (i + 1);
                    break;
                }
                if (prev == null || prev != stmt) // do not redundantly re-save the most previously executed command to the history
                {
                    try
                    {
                        using (StreamWriter sw = File.AppendText(this.HistoryPath))
                        {
                            sw.WriteLine("- " + seq.ToString() + ":");
                            sw.WriteLine("\ttime: " + stmt.Time.ToString());
                            sw.WriteLine("\tstmt: " + stmt.Statement);
                            if (!string.IsNullOrWhiteSpace(stmt.Expansion))
                                sw.WriteLine("\texpd: " + stmt.Expansion);
                        }
                        this.History[seq + 1] = stmt;
                    }
                    catch
                    {
                        this.AddError("Unable to save history as requested");
                    }
                }
            }
            else
            {
                this.AddError("Cannot add an invalid command to command history.");
            }
        }
        public void ReadAllHistory()
        {
            UInt32 seq = 0;
            var restored = true;
            string time = string.Empty;
            string stmt = string.Empty;
            string expd = string.Empty;
            try
            {
                var lines = File.ReadLines(this.HistoryPath);

                foreach (string line in lines)
                {
                    if (line.StartsWith("- "))
                    {
                        if (!restored)
                        {
                            var estmt = new QExpandableStatement { Expansion = expd, Statement = stmt, Time = Int64.Parse(time) };
                            this.History[seq] = estmt;
                        }
                        seq = UInt32.Parse(line.Substring(2, line.Length - 3));
                        time = string.Empty;
                        stmt = string.Empty;
                        expd = string.Empty;
                        restored = false;
                    }
                    else if (line.StartsWith("\ttime: "))
                    {
                        time = line.Substring(7);
                    }
                    else if (line.StartsWith("\tstmt: "))
                    {
                        stmt = line.Substring(7);
                    }
                    else if (line.StartsWith("\texpd: "))
                    {
                        expd = line.Substring(7);
                    }
                }
            }
            catch
            {
                this.AddError("Unable to read History.quelle");
            }
            if (!restored)
            {
                var estmt = new QExpandableStatement { Expansion = expd, Statement = stmt, Time = Int64.Parse(time) };
                this.History[seq] = estmt;
            }
        }
        public IEnumerable<(UInt32 seq, QExpandableStatement entry)> GetHistory(UInt32 minSeq = 0, UInt32 maxSeq = UInt32.MaxValue, DateTime? notBefore = null, DateTime? notAfter = null)
        {
            var notBeforeOffset = notBefore != null ? new DateTimeOffset(notBefore.Value) : DateTimeOffset.MinValue;
            var notAfterOffset  = notAfter  != null ? new DateTimeOffset(notAfter.Value)  : DateTimeOffset.MaxValue;

            var notBeforeLong = notBeforeOffset.ToUnixTimeMilliseconds();
            var notAfterLong  = notAfterOffset.ToUnixTimeMilliseconds();

            foreach (var entry in this.History)
            { 
                if((entry.Key >= minSeq && entry.Key <= maxSeq)
                && (entry.Value.Time >= notBeforeLong && entry.Value.Time <= notAfterLong))
                {
                    yield return (entry.Key, entry.Value);
                }
            }
        }
        public QExpandableStatement? GetHistoryEntry(UInt32 sequence)
        {
            if (sequence > 0 && sequence < UInt32.MaxValue)
            {
                var history = this.GetHistory(minSeq: sequence, maxSeq: sequence);

                QExpandableStatement? found = null;
                int cnt = 0;
                foreach (var candidate in history)
                {
                    found = candidate.entry;
                    if (++cnt > 2)
                        break;
                }
                if (cnt == 1)
                    return found;
            }
            return null;
        }
        public void AddMacro(QExpandableStatement stmt, string label)
        {
            if (!string.IsNullOrEmpty(label))
            {
                if (!(string.IsNullOrWhiteSpace(stmt.Statement) || string.IsNullOrWhiteSpace(stmt.Expansion)))
                {

                    var macro = Path.Combine(this.MacroPath, label + ".quelle");

                    try
                    {
                        using (var file = new StreamWriter(File.Create(macro)))
                        {
                            file.WriteLine("time: " + stmt.Time.ToString());
                            file.WriteLine("stmt: " + stmt.Statement);
                            file.WriteLine("expd: " + stmt.Expansion);
                        }
                    }
                    catch
                    {
                        this.AddError("Cannot create macro: " + label);
                    }
                }
                else
                {
                    this.AddError("Cannot save an invalid command as a macro.");
                }
            }
            else
            {
                this.AddError("Cannot save a command as a macro without a label.");
            }
        }
        public QExpandableStatement GetMacro(string label)
        {
            var macro = Path.Combine(this.MacroPath, label + ".quelle");

            var lines = File.ReadLines(this.HistoryPath);

            string time = string.Empty;
            string stmt = string.Empty;
            string expd = string.Empty;
            foreach (string line in lines)
            {
                if (line.StartsWith("\ttime: "))
                {
                    time = line.Substring(7);
                }
                else if (line.StartsWith("\tstmt: "))
                {
                    stmt = line.Substring(7);
                }
                else if (line.StartsWith("\texpd: "))
                {
                    expd = line.Substring(7);
                }
            }
            var estmt = new QExpandableStatement { Expansion = expd, Statement = stmt, Time = Int64.Parse(time) };

            return estmt;
        }
    }
}