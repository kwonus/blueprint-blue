namespace Blueprint.Blue
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Blueprint.FuzzyLex;
    using AVXLib;
    
    public interface IDiagnostic
    {
        void AddError(string message);
        void AddWarning(string message);
    }

    public class QContext: IDiagnostic
    {
        public QSettings GlobalSettings { get; internal set; }
        public static string Home { get; private set; }
        public uint InvocationCount     { get; internal set; }

        public UInt16[]?Fields { get; set; }

        public QStatement Statement { get; private set; }

        static QContext()
        {
            QContext.Home = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AV-Bible");
            BlueprintLex.Initialize(ObjectTable.AVXObjects);
        }
        public static string HistoryPath
        {
            get
            {
                if (!Directory.Exists(QContext.Home))
                    Directory.CreateDirectory(QContext.Home);
                return Path.Combine(QContext.Home, "history.yaml").Replace("\\", "/");
            }
        }
        public static string MacroPath
        {
            get
            {
                if (!Directory.Exists(QContext.Home))
                    Directory.CreateDirectory(QContext.Home);
                string labels = Path.Combine(QContext.Home, "Labels").Replace("\\", "/");

                if (!Directory.Exists(labels))
                    Directory.CreateDirectory(labels);

                return labels;
            }
        }

        public Dictionary<long, ExpandableHistory> History { get; private set; }
        public Dictionary<string, ExpandableMacro> Macros { get; private set; }

        public QContext(QStatement statement)
        {
            Blueprint.FuzzyLex.BlueprintLex.Initialize(ObjectTable.AVXObjects);
            this.Statement = statement;
            this.InvocationCount = 0; // This can be updated when Create() is called on Implicit clauses

            this.Fields  = null;    // Null means that no fields were provided; In Quelle, this is different than an empty array of fields

            if (string.IsNullOrEmpty(QContext.Home))
            {
                this.AddWarning("A session context cannot be established");
            }
            this.GlobalSettings = new QSettings(Path.Combine(QContext.Home, "settings.yaml"));

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
            ExpandableInvocation.YamlSerializer(QContext.HistoryPath, this.History);    // highly inefficient, but ok for v1
        }
        public void ReadAllHistory()
        {
            if (File.Exists(QContext.HistoryPath))
            {
                this.History = ExpandableHistory.YamlDeserializer(QContext.HistoryPath);
            }
            else
            {
                this.History = new();
            }
        }
        public IEnumerable<ExpandableHistory> GetHistory(UInt32 idAfter = 0, UInt32 idBefore = UInt32.MaxValue, DateTime? dateAfter = null, DateTime? dateBefore = null)
        {
            var dateAfterOffset  = dateAfter  != null ? new DateTimeOffset(dateAfter.Value)  : DateTimeOffset.MinValue;
            var dateBeforeOffset = dateBefore != null ? new DateTimeOffset(dateBefore.Value) : DateTimeOffset.MaxValue;

            long dateBeforeLong = dateAfterOffset.ToFileTime();
            long dateAfterLong  = dateBeforeOffset.ToFileTime();

            foreach (var entry in this.History)
            { 
                if((entry.Value.Id >= idAfter && entry.Value.Id <= idBefore)
                && (entry.Value.Time >= dateBeforeLong && entry.Value.Time <= dateAfterLong))
                {
                    yield return entry.Value;
                }
            }
        }
        public ExpandableInvocation? GetHistoryEntry(UInt32 sequence)
        {
            if (sequence > 0 && sequence < UInt32.MaxValue)
            {
                var history = this.GetHistory(idAfter: sequence, idBefore: sequence);

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
            if (Directory.Exists(QContext.MacroPath))
            {
                this.Macros = ExpandableMacro.YamlDeserializer(QContext.MacroPath);
            }
        }
        public void AddMacro(ExpandableMacro macro)
        {
            if (!string.IsNullOrEmpty(macro.Label))
            {
                var yaml = Path.Combine(QContext.MacroPath, macro.Label + ".yaml");
                ExpandableInvocation.YamlSerializer(yaml, macro);
            }
        }
        public ExpandableInvocation GetMacro(string label)
        {
            var macro = Path.Combine(QContext.MacroPath, label + ".yaml");

            var lines = File.ReadLines(QContext.HistoryPath);

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
        public IEnumerable<ExpandableMacro> GetMacros(string? spec, DateTime? notBefore = null, DateTime? notAfter = null)
        {
            var notBeforeOffset = notBefore != null ? new DateTimeOffset(notBefore.Value) : DateTimeOffset.MinValue;
            var notAfterOffset = notAfter != null ? new DateTimeOffset(notAfter.Value) : DateTimeOffset.MaxValue;

            var notBeforeLong = notBeforeOffset.ToFileTime();
            var notAfterLong = notAfterOffset.ToFileTime();

            foreach (var entry in this.Macros)
            {
                if (entry.Value.Time >= notBeforeLong && entry.Value.Time <= notAfterLong)
                {
                    if (string.IsNullOrWhiteSpace(spec))
                    {
                        yield return entry.Value;
                    }
                    else if (!spec.Contains('*'))
                    {
                        if (entry.Value.Label.Equals(spec, StringComparison.InvariantCultureIgnoreCase))
                            yield return entry.Value;
                    }
                    else
                    {
                        // TO DO: use QWildcard and do match tests
                        /*
                        bool matches = true;

                        for (int i = 0; i < pieces.Length; i++)
                        {
                            if (contains[i])
                                matches = entry.Value.Label.Contains(spec, StringComparison.InvariantCultureIgnoreCase);
                            else if (beginswith[i])
                                matches = entry.Value.Label.StartsWith(spec, StringComparison.InvariantCultureIgnoreCase);
                            else if (endswith[i])
                                matches = entry.Value.Label.EndsWith(spec, StringComparison.InvariantCultureIgnoreCase);
                            if (!matches)
                                break;
                        }
                        if (matches)
                            yield return entry.Value;
                        */
                    }
                }
            }
        }
    }
}