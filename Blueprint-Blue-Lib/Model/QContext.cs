namespace Blueprint.Blue
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Blueprint.FuzzyLex;
    using AVXLib;
    using System.Runtime.CompilerServices;
    using System.Reflection.Emit;
    using YamlDotNet.Core;

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

            QContext.ReadAllHistory();
            QContext.ReadAllMacros();
        }
        public static string SettingsFile
        {
            get
            {
                if (!Directory.Exists(QContext.Home))
                    Directory.CreateDirectory(QContext.Home);
                return Path.Combine(QContext.Home, "settings.yaml").Replace("\\", "/");
            }
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
                string labels = Path.Combine(QContext.Home, "Macros").Replace("\\", "/");

                if (!Directory.Exists(labels))
                    Directory.CreateDirectory(labels);

                return labels;
            }
        }

        public static Dictionary<Int64, ExpandableHistory> History { get; private set; }
        public static Dictionary<string, ExpandableMacro> Macros { get; private set; }

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
        }
        public void AddError(string message)
        {
            this.Statement.AddError(message);
        }
        public ExpandableInvocation? Expand(UInt32 seq)     // e.g. $1
        {
            return QContext.GetHistoryEntry(seq);
        }
        public ExpandableInvocation? Expand(string label)   // e.g. $my-macro-def
        {
            return QContext.GetMacro(label);
        }
        public void AddWarning(string message)
        {
            this.Statement.AddWarning(message);
        }
        public void AddHistory(ExpandableHistory item)
        {
            QContext.History[item.Time] = item;
            item.Id = (UInt64)(QContext.History.Count + 1);
            ExpandableInvocation.HistoryAppendSerializer(QContext.HistoryPath, item);    // highly inefficient, but ok for v1
        }
        public static void ReadAllHistory()
        {
            if (File.Exists(QContext.HistoryPath))
            {
                QContext.History = ExpandableHistory.HistoryDeserializer(QContext.HistoryPath);
            }
            else
            {
                QContext.History = new();
            }
            // Re-number the entries
            //
            UInt64 id = 1;
            foreach (var entry in QContext.History.Values)
            {
                entry.Id = id++;
            }
        }
        public static void DeleteHistory(UInt32 ifFrom = 0, UInt32 idUnto = UInt32.MaxValue, DateTime? notBefore = null, DateTime? notAfter = null)
        {
            UInt64 id = 1;
            foreach (var entry in QContext.History.Values)
            {
                entry.Id = id++;
            }

            var notBeforeOffset = notBefore != null ? new DateTimeOffset(notBefore.Value) : DateTimeOffset.MinValue;
            var notAfterOffset = notAfter != null ? new DateTimeOffset(notAfter.Value) : DateTimeOffset.MaxValue;

            long notBeforeLong = notBeforeOffset.ToFileTime();
            long notAfterLong = notAfterOffset.ToFileTime();

            var old = QContext.History;
            QContext.History = new();
            foreach (ExpandableHistory entry in old.Values)
            {
                if ((entry.Id >= ifFrom && entry.Id <= idUnto)
                && (entry.Time >= notBeforeLong && entry.Time <= notAfterLong))
                {
                    continue;
                }
                QContext.History[(int)entry.Id] = entry;
            }
            // Re-number the entries
            //
            id = 1;
            foreach (var entry in QContext.History.Values)
            {
                entry.Id = id++;
            }
            ExpandableInvocation.YamlSerializer(QContext.History);
        }

        public static IEnumerable<ExpandableHistory> GetHistory(UInt32 ifFrom = 0, UInt32 idUnto = UInt32.MaxValue, DateTime? notBefore = null, DateTime? notAfter = null)
        {
            UInt64 id = 1;
            foreach (var entry in QContext.History.Values)
            {
                entry.Id = id++;
            }

            var notBeforeOffset  = notBefore  != null ? new DateTimeOffset(notBefore.Value)  : DateTimeOffset.MinValue;
            var notAfterOffset = notAfter != null ? new DateTimeOffset(notAfter.Value) : DateTimeOffset.MaxValue;

            long notBeforeLong  = notBeforeOffset.ToFileTime();
            long notAfterLong = notAfterOffset.ToFileTime();

            foreach (var entry in QContext.History.Values)
            { 
                if((entry.Id >= ifFrom && entry.Id <= idUnto)
                && (entry.Time >= notBeforeLong && entry.Time <= notAfterLong))
                {
                    yield return entry;
                }
            }
        }
        public static ExpandableInvocation? GetHistoryEntry(UInt64 sequence)
        {
            if (sequence > 0 && sequence < UInt32.MaxValue)
            {
                foreach (var candidate in QContext.History.Values)
                {
                    if (candidate.Id == sequence)
                        return candidate;
                }
            }
            return null;
        }
        public static void AppendHistory(ExpandableHistory history)
        {
            if (history.Id > UInt64.MinValue && history.Id < UInt64.MaxValue)
            {
                var yaml = QContext.HistoryPath;
                history.Id = (UInt64)(QContext.History.Count + 1);
                ExpandableInvocation.HistoryAppendSerializer(yaml, history);
            }
        }
        public static void ReadAllMacros()
        {
            if (Directory.Exists(QContext.MacroPath))
            {
                QContext.Macros = ExpandableMacro.YamlDeserializer(QContext.MacroPath);
            }
        }
        public static void AddMacro(ExpandableMacro macro)
        {
            if (!string.IsNullOrEmpty(macro.Label))
            {
                var yaml = Path.Combine(QContext.MacroPath, macro.Label + ".yaml");
                ExpandableInvocation.YamlSerializer(yaml, macro);
            }
        }
        public static ExpandableInvocation? GetMacro(string label)
        {
            string macro = Path.Combine(QContext.MacroPath, label + ".yaml");
            return ExpandableHistory.MacroDeserializer(macro);
        }
        private static string GetMacroFile(string label)
        {
            return Path.Combine(QContext.MacroPath, label + ".yaml");
        }
        private static bool IsMatch(ExpandableMacro macro, long notBefore, long notAfter, string? wildcard = null)
        {
            if (macro.Time >= notBefore && macro.Time <= notAfter)
            {
                if (wildcard == null)
                {
                    return true;
                }
                string[] parts = ('<' + wildcard + '>').Split('*');
                bool match = true;
                if (parts.Length >= 2)
                {
                    foreach (string part in parts)
                    {
                        if (string.IsNullOrWhiteSpace(part))
                            continue;
                        if (part.StartsWith('<'))
                            match = macro.Label.StartsWith(part.Substring(1), StringComparison.InvariantCultureIgnoreCase);
                        if (part.EndsWith('>'))
                            match = macro.Label.StartsWith(part.Substring(0, part.Length - 1), StringComparison.InvariantCultureIgnoreCase);
                        else
                            match = macro.Label.Contains(part, StringComparison.InvariantCultureIgnoreCase);
                        if (!match)
                            break;
                    }
                    return match;
                }
            }
            return false;
        }
        private static void DeleteMacro(ExpandableMacro macro, long notBefore, long notAfter, string? wildcard = null)
        {
            try
            {
                if (macro.Time >= notBefore && macro.Time <= notAfter)
                {
                    if (wildcard == null)
                    {
                        File.Delete(GetMacroFile(macro.Label));
                        return;
                    }
                    string[] parts = ('<' + wildcard + '>').Split('*');
                    bool match = true;
                    if (parts.Length >= 2)
                    {
                        foreach (string part in parts)
                        {
                            if (string.IsNullOrWhiteSpace(part))
                                continue;
                            if (part.StartsWith('<'))
                                match = macro.Label.StartsWith(part.Substring(1), StringComparison.InvariantCultureIgnoreCase);
                            if (part.EndsWith('>'))
                                match = macro.Label.StartsWith(part.Substring(0, part.Length-1), StringComparison.InvariantCultureIgnoreCase);
                            else
                                match = macro.Label.Contains(part, StringComparison.InvariantCultureIgnoreCase);
                            if (!match)
                                break;
                        }
                        if (match)
                            File.Delete(GetMacroFile(macro.Label));
                    }
                }
            }
            catch { ; }

        }
        public static void DeleteMacros(string? spec, DateTime? notBefore = null, DateTime? notAfter = null)
        {
            DateTimeOffset notBeforeOffset = notBefore != null ? new DateTimeOffset(notBefore.Value) : DateTimeOffset.MinValue;
            DateTimeOffset notAfterOffset  = notAfter  != null ? new DateTimeOffset(notAfter.Value)  : DateTimeOffset.MaxValue;

            long notBeforeLong = notBeforeOffset.ToFileTime();
            long notAfterLong  = notAfterOffset.ToFileTime();

            if ((spec != null) && !spec.Contains('*'))
            {
                string norm = spec.Trim().ToLower();
                if (QContext.Macros.ContainsKey(norm))
                {
                    DeleteMacro(QContext.Macros[norm], notBeforeLong, notAfterLong, spec);
                    QContext.Macros.Remove(norm);
                    return;
                }
            }
            foreach (ExpandableMacro entry in QContext.Macros.Values)
            {
                if (entry.Time >= notBeforeLong && entry.Time <= notAfterLong)
                {
                    if (string.IsNullOrWhiteSpace(spec))
                    {
                        continue;
                    }
                    DeleteMacro(entry, notBeforeLong, notAfterLong, spec);
                }
                else
                {
                    DeleteMacro(entry, notBeforeLong, notAfterLong, spec);
                }
            }
            QContext.ReadAllMacros();
        }
        public static IEnumerable<ExpandableMacro> GetMacros(string? spec, DateTime? notBefore = null, DateTime? notAfter = null)
        {
            var notBeforeOffset = notBefore != null ? new DateTimeOffset(notBefore.Value) : DateTimeOffset.MinValue;
            var notAfterOffset = notAfter != null ? new DateTimeOffset(notAfter.Value) : DateTimeOffset.MaxValue;

            var notBeforeLong = notBeforeOffset.ToFileTime();
            var notAfterLong = notAfterOffset.ToFileTime();

            if ((spec != null) && !spec.Contains('*'))
            {
                string norm = spec.Trim().ToLower();
                if (QContext.Macros.ContainsKey(norm))
                {
                    ExpandableMacro candidate = QContext.Macros[norm];
                    if (QContext.IsMatch(candidate, notBeforeLong, notAfterLong))
                    {
                        yield return candidate;
                    }
                }
            }
            else foreach (var entry in QContext.Macros.Values)
            {
                if (entry.Time >= notBeforeLong && entry.Time <= notAfterLong)
                {
                    if (string.IsNullOrWhiteSpace(spec))
                    {
                        yield return entry;
                    }
                    else if (QContext.IsMatch(entry, notBeforeLong, notAfterLong, spec))
                    {
                        yield return entry;
                    }
                }
            }
        }
    }
}