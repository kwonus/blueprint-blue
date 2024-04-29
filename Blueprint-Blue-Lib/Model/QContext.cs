using Blueprint.Blue;

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
    using BlueprintBlue.Model;
    using System.Text;

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

        public MemoryStream InternalExportStream { get; internal set; }

        static QContext()
        {
            QContext.Home = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AV-Bible");
            BlueprintLex.Initialize(ObjectTable.AVXObjects);
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
                return Path.Combine(QContext.Home, "History").Replace("\\", "/");
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
            this.InternalExportStream = new();
        }
        public void AddError(string message)
        {
            this.Statement.AddError(message);
        }
        public ExpandableInvocation? Expand(string label)   // e.g. $my-macro-def
        {
            if (label.Length > 0)
            {
                if (label[0] >= '0' && label[0] <= '9')
                    return ExpandableHistory.Deserialize(label);
                else
                    return ExpandableMacro.Deserialize(label);
            }
            return null;
        }
        public void AddWarning(string message)
        {
            this.Statement.AddWarning(message);
        }

        public static void DeleteHistory(UInt32 ifFrom = 0, UInt32 idUnto = UInt32.MaxValue, UInt32? notBefore = null, UInt32? notAfter = null)
        {
            // TO DO: DELETE HISTORY (4/10/2024)
            /*
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
            */
        }
        public const UInt32 DefaultHistoryCount = 25;

        public static IEnumerable<ExpandableInvocation> GetHistory(UInt32? rangeFrom, UInt32? rangeUnto)
        {
            string folder = QContext.HistoryPath;

            UInt32 from = rangeFrom.HasValue ? rangeFrom.Value : 1999_12_31;
            UInt32 unto = rangeUnto.HasValue ? rangeUnto.Value : 2200_01_01;

            UInt32 from_yr = from / (100 * 100);

            foreach (string year in from yyyy in Directory.EnumerateDirectories(folder, "2???") orderby yyyy ascending select Path.GetFileName(yyyy))
            {
                UInt32 y = 0;
                UInt32 yr = 0;
                try
                {
                    yr = (UInt32) UInt16.Parse(year);
                    y = (UInt32)(yr * 100 * 100);
                }
                catch
                {
                    continue;
                }
                if (from_yr > yr)
                    continue;
                if (unto < y)
                    break;

                string YYYY = Path.Combine(folder, year);
                foreach (string month in from mm in Directory.EnumerateDirectories(YYYY) orderby mm ascending select Path.GetFileName(mm))
                {
                    string MM = Path.Combine(YYYY, month);
                    UInt32 m = 0;
                    UInt32 mo = 0;
                    UInt32 yr_mo = (yr * 100);
                    try
                    {
                        mo = (UInt32)byte.Parse(month);
                        yr_mo += mo;
                        m = (100 * yr_mo);
                    }
                    catch
                    {
                        continue;
                    }
                    if (from / 100 > yr_mo)
                        continue;
                    if (unto < m)
                        goto done;

                    foreach (string day in from dd in Directory.EnumerateDirectories(MM) orderby dd ascending select Path.GetFileName(dd))
                    {
                        string DD = Path.Combine(MM, day);
                        UInt32 d = 0;
                        try
                        {
                            d = m + byte.Parse(day);
                        }
                        catch
                        {
                            continue;
                        }
                        if (from > d)
                            continue;
                        if (unto < d)
                            goto done;

                        foreach (string sequence in from seq in Directory.EnumerateFiles(DD, "*.yaml") orderby seq ascending select seq.Replace('\\', '/'))
                        {
                            ExpandableInvocation? invocation = ExpandableHistory.Deserialize(yaml:sequence);
                            if (invocation != null)
                            {
                                yield return invocation;
                            }
                        }
                    }
                }
            }
        done:
            ;
        }

        public static ExpandableInvocation? GetHistoryEntry(string tag)
        {
            return ExpandableHistory.Deserialize(tag);
        }
        public static void AppendHistory(ExpandableHistory history)
        {
            history.Serialize();
        }
        public static ExpandableInvocation? GetMacro(string label)
        {
            return ExpandableMacro.Deserialize(label);
        }
        private static string GetMacroFile(string label)
        {
            return Path.Combine(QContext.MacroPath, label + ".yaml");
        }
        private static bool IsMatch(ExpandableMacro macro, UInt32 notBefore, UInt32 notAfter, string? wildcard = null)
        {
            UInt32 numeric = macro.GetDateNumeric();
            if (numeric >= notBefore && numeric <= notAfter)
            {
                if (wildcard == null)
                {
                    return true;
                }
                string label = macro.Tag;
                string[] parts = ('<' + wildcard + '>').Split('*');
                bool match = true;
                if (parts.Length >= 2)
                {
                    foreach (string part in parts)
                    {
                        if (string.IsNullOrWhiteSpace(part))
                            continue;
                        if (part.StartsWith('<'))
                            match = label.StartsWith(part.Substring(1), StringComparison.InvariantCultureIgnoreCase);
                        if (part.EndsWith('>'))
                            match = label.StartsWith(part.Substring(0, part.Length - 1), StringComparison.InvariantCultureIgnoreCase);
                        else
                            match = label.Contains(part, StringComparison.InvariantCultureIgnoreCase);
                        if (!match)
                            break;
                    }
                    return match;
                }
            }
            return false;
        }
        public static IEnumerable<ExpandableInvocation> GetMacros(string wildcard)
        {
            foreach (string yaml in Directory.EnumerateFiles(QContext.MacroPath, "*.yaml"))
            {
                ExpandableInvocation? macro = null;
                try
                {
                    string label = Path.GetFileNameWithoutExtension(yaml);
                    string[] parts = ('<' + wildcard + '>').Split('*');
                    bool match = true;
                    if (parts.Length >= 2)
                    {
                        foreach (string part in parts)
                        {
                            if (string.IsNullOrWhiteSpace(part))
                                continue;
                            if (part.StartsWith('<'))
                                match = (part.Length == 1) || label.StartsWith(part.Substring(1), StringComparison.InvariantCultureIgnoreCase);
                            else if (part.EndsWith('>'))
                                match = (part.Length == 1) || label.StartsWith(part.Substring(0, part.Length - 1), StringComparison.InvariantCultureIgnoreCase);
                            else
                                match = label.Contains(part, StringComparison.InvariantCultureIgnoreCase);
                            if (!match)
                                break;
                        }
                        if (match)
                        {
                            macro = ExpandableMacro.Deserialize(label);
                        }
                    }
                }
                catch { ; }
                if (macro != null)
                    yield return macro;
            }
        }
        public static void DeleteMacros(IEnumerable<ExpandableInvocation> removals)
        {
            foreach (ExpandableInvocation macro in removals)
            {
                string yaml = macro.Tag + ".yaml";
                try
                {
                    File.Delete(Path.Combine(QContext.MacroPath, yaml));
                }
                catch
                {
                    ;
                }
            }
        }
        public static IEnumerable<ExpandableInvocation> GetMacros(UInt32? notBefore = null, UInt32? notAfter = null)
        {
            foreach (string yaml in Directory.EnumerateFiles(QContext.MacroPath, "*.yaml"))
            {
                string label = Path.GetFileNameWithoutExtension(yaml);

                ExpandableInvocation? macro = null;
                try
                {
                    macro = ExpandableMacro.Deserialize(label);

                    if (macro != null)
                    {
                        if (notBefore != null || notAfter != null)
                        {
                            UInt32 time = macro.GetDateNumeric();
                            if (notBefore.HasValue && time < notBefore.Value)
                                continue;
                            if (notAfter.HasValue && time > notAfter.Value)
                                continue;
                        }
                    }
                }
                catch { ; }

                if (macro != null)
                {
                    yield return macro;
                }
            }
        }
    }
}