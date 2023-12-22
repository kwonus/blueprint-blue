namespace Blueprint.Blue
{
    using BlueprintBlue;
    using System.Linq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using BlueprintBlue.FuzzyLex;
    using AVXLib;
    using System.Text.Json.Serialization;
    
    public interface IDiagnostic
    {
        void AddError(string message);
        void AddWarning(string message);
    }

    public class QContext: IDiagnostic
    {
        public QSettings GlobalSettings { get; internal set; }
        public string Home { get; internal set; }
        public uint InvocationCount     { get; internal set; }

        public UInt16[]?Fields { get; set; }

        public QStatement Statement { get; private set; }

        public string HistoryPath { get; private set; } // not used yet
        public string MacroPath { get; private set; }   // not used yet

        static QContext()
        {
            BlueprintLex.Initialize(ObjectTable.AVXObjects);
        }
        public Dictionary<long, ExpandableHistory> History { get; private set; }
        public Dictionary<string, ExpandableMacro> Macros { get; private set; }

        public QContext(QStatement statement)
        {
            BlueprintBlue.FuzzyLex.BlueprintLex.Initialize(ObjectTable.AVXObjects);
            this.Statement = statement;
            this.InvocationCount = 0; // This can be updated when Create() is called on Implicit clauses

            this.Fields  = null;    // Null means that no fields were provided; In Quelle, this is different than an empty array of fields
            this.HistoryPath = string.Empty;
            this.MacroPath = string.Empty;
            this.History = new();
            this.Macros = new();

            this.Home = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AV-Bible");

            if (!string.IsNullOrEmpty(this.Home))
            {
                if (!Directory.Exists(this.Home))
                    Directory.CreateDirectory(this.Home);

                this.HistoryPath = Path.Combine(this.Home, "history.yaml").Replace("\\", "/");
                this.MacroPath = Path.Combine(this.Home, "Labels").Replace("\\", "/");
            }
            else
            {
                this.AddWarning("A session context cannot be established");
            }
            this.GlobalSettings = new QSettings(Path.Combine(this.Home, "settings.yaml"));

            if (!ObjectTable.AVXObjects.Mem.valid)
            {
                this.AddError("Unable to load AVX Data. Without this library, other things will break");
            }
            this.ReadAllHistory();
            this.ReadAllMacros();
        }
        public void AddError(string message)
        {
            this.Statement.AddError(message);
        }
        public ExpandableInvocation? Expand(UInt32 seq)     // e.g. $1
        {
            return this.GetHistoryEntry(seq);
        }
        public ExpandableInvocation? Expand(string label)   // e.g. $my-macro-def
        {
            return this.GetMacro(label);
        }
        public void AddWarning(string message)
        {
            this.Statement.AddWarning(message);
        }
        public void AddHistory(ExpandableHistory item)
        {
            this.History[item.Time] = item;
            ExpandableInvocation.YamlSerializer(this.HistoryPath, this.History);    // highly inefficient, but ok for v1
        }
        public void ReadAllHistory()
        {
            if (File.Exists(this.HistoryPath))
            {
                this.History = ExpandableHistory.YamlDeserializer(this.HistoryPath);
            }
        }
        public IEnumerable<ExpandableHistory> GetHistory(UInt32 minSeq = 0, UInt32 maxSeq = UInt32.MaxValue, DateTime? notBefore = null, DateTime? notAfter = null)
        {
            var notBeforeOffset = notBefore != null ? new DateTimeOffset(notBefore.Value) : DateTimeOffset.MinValue;
            var notAfterOffset  = notAfter  != null ? new DateTimeOffset(notAfter.Value)  : DateTimeOffset.MaxValue;

            var notBeforeLong = notBeforeOffset.ToUnixTimeMilliseconds();
            var notAfterLong  = notAfterOffset.ToUnixTimeMilliseconds();

            foreach (var entry in this.History)
            { 
                if((entry.Value.Id >= minSeq && entry.Value.Id <= maxSeq)
                && (entry.Value.Time >= notBeforeLong && entry.Value.Time <= notAfterLong))
                {
                    yield return entry.Value;
                }
            }
        }
        public ExpandableInvocation? GetHistoryEntry(UInt32 sequence)
        {
            if (sequence > 0 && sequence < UInt32.MaxValue)
            {
                var history = this.GetHistory(minSeq: sequence, maxSeq: sequence);

                ExpandableInvocation? found = null;
                int cnt = 0;
                foreach (var candidate in history)
                {
                    //found = candidate.entry;
                    if (++cnt > 2)
                        break;
                }
                if (cnt == 1)
                    return found;
            }
            return null;
        }
        public void ReadAllMacros()
        {
            if (Directory.Exists(this.MacroPath))
            {
                this.Macros = ExpandableMacro.YamlDeserializer(this.MacroPath);
            }
        }
        public void AddMacro(ExpandableMacro macro)
        {
            if (!string.IsNullOrEmpty(macro.Label))
            {
                var yaml = Path.Combine(this.MacroPath, macro.Label + ".yaml");
                ExpandableInvocation.YamlSerializer(yaml, macro);
            }
        }
        public ExpandableInvocation GetMacro(string label)
        {
            var macro = Path.Combine(this.MacroPath, label + ".yaml");

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
            var estmt = new ExpandableInvocation(); // { Expansion = expd, Statement = stmt, Time = Int64.Parse(time) };

            return estmt;
        }
        public IEnumerable<ExpandableMacro> GetMacros(string? wildcard, DateTime? notBefore = null, DateTime? notAfter = null)
        {
            var notBeforeOffset = notBefore != null ? new DateTimeOffset(notBefore.Value) : DateTimeOffset.MinValue;
            var notAfterOffset = notAfter != null ? new DateTimeOffset(notAfter.Value) : DateTimeOffset.MaxValue;

            var notBeforeLong = notBeforeOffset.ToUnixTimeMilliseconds();
            var notAfterLong = notAfterOffset.ToUnixTimeMilliseconds();

            bool equals = (!string.IsNullOrEmpty(wildcard)) && (wildcard.IndexOf('*') < 0);
            string[] pieces = equals ? [wildcard] : wildcard != null ? wildcard.Split('*', StringSplitOptions.RemoveEmptyEntries) : new string[0];
            bool[] contains   = new bool[pieces.Length];
            bool[] beginswith = new bool[pieces.Length];
            bool[] endswith   = new bool[pieces.Length];
            if ((pieces.Length > 0) && !equals)
            {
                for (int i = 0; i < pieces.Length; i++)
                {
                    contains[i] = false;
                    beginswith[i] = false;
                    endswith[i] = false;
                }
                for (int i = 0; i < pieces.Length; i++)
                {
                    if (i == 0)
                    {
                        if (pieces.Length == 1)
                        {
                            if (wildcard.StartsWith('*'))
                            {
                                if (wildcard.EndsWith('*'))
                                    contains[i] = true;
                                else
                                    beginswith[i] = true;
                            }
                            else if (wildcard.EndsWith('*'))
                            {
                                endswith[i] = true;
                            }
                        }
                    }
                    else if (i == pieces.Length - 1)
                    {
                        if (wildcard.EndsWith('*'))
                            contains[i] = true;
                        else
                            endswith[i] = true;
                    }
                    else
                    {
                        contains[i] = true;
                    }
                }
            }
            foreach (var entry in this.Macros)
            {
                if (entry.Value.Time >= notBeforeLong && entry.Value.Time <= notAfterLong)
                {
                    if (wildcard == null)
                    {
                        yield return entry.Value;
                    }
                    else if (equals)
                    {
                        if (entry.Value.Label.Equals(wildcard, StringComparison.InvariantCultureIgnoreCase))
                            yield return entry.Value;
                    }
                    else
                    {
                        bool matches = true;

                        for (int i = 0; i < pieces.Length; i++)
                        {
                            if (contains[i])
                                matches = entry.Value.Label.Contains(wildcard, StringComparison.InvariantCultureIgnoreCase);
                            else if (beginswith[i])
                                matches = entry.Value.Label.StartsWith(wildcard, StringComparison.InvariantCultureIgnoreCase);
                            else if (endswith[i])
                                matches = entry.Value.Label.EndsWith(wildcard, StringComparison.InvariantCultureIgnoreCase);
                            if (!matches)
                                break;
                        }
                        if (matches)
                            yield return entry.Value;
                    }
                }
            }
        }

    }
}