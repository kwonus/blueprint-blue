namespace Blueprint.Blue
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Blueprint.Model.Implicit;
    using AVSearch.Interfaces;
    using Pinshot.Blue;
    using YamlDotNet.Serialization;
    using AVXLib.Memory;
    using static System.Net.Mime.MediaTypeNames;
    using static System.Runtime.InteropServices.JavaScript.JSType;
    using System.Runtime.Intrinsics.X86;
    using System.Text.RegularExpressions;
    using AVXLib.Framework;
    using System.Diagnostics.CodeAnalysis;
    using System;

    public class QSettings: ISettings
    {
        public string GetAll()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("span:       "); builder.AppendLine(this.Span.ToString());
            builder.Append("lexicon:    "); builder.AppendLine(this.Lexicon.ToString());
            builder.Append("display:    "); builder.AppendLine(this.Display.ToString());
            builder.Append("format:     "); builder.AppendLine(this.Format.ToString());
            builder.Append("similarity: "); builder.AppendLine(this.Similarity.ToString());
            builder.Append("version:    "); builder.Append(Pinshot_RustFFI.VERSION);

            return builder.ToString();
        }
        public string Get(QGet setting)
        {
            string key = (setting.Key.Length >= 1 && setting.Key[0] == '%') ? setting.Key.Substring(1) : setting.Key;
            switch (key)
            {
                case "span":       return this.Span.ToString();
                case "lexicon.search":
                case "search":     return this.Lexicon.ToString();
                case "lexicon.render":
                case "render":     return this.Display.ToString();
                case "format":     return this.Format.ToString();
                case "similarity": return this.Similarity.ToString();
                case "version":    return Pinshot_RustFFI.VERSION;
                case "all":        return this.GetAll();          
            }

            return this.GetAll();
        }
        public bool Assign(QAssign assignment)
        {
            string key = (assignment.Key.Length >= 1 && assignment.Key[0] == '%') ? assignment.Key.Substring(1) : assignment.Key;
            switch (key)
            {
                case "span":       this.Span = new QSpan(assignment.Value);              return true;   
                case "lexicon.search":
                case "search":     this.Lexicon = new QLexicalDomain(assignment.Value); return true;
                case "lexicon.render":
                case "render":     this.Display = new QLexicalDisplay(assignment.Value); return true;
                case "format":     this.Format = new QFormat(assignment.Value);          return true;
                case "similarity": this.Similarity = new QSimilarity(assignment.Value, this);  return true;
            }
            return false;
        }
        public bool Set(QSet setting)
        {
            switch (setting.Key)
            {
                case "span":       this.Span = new QSpan(setting.Value);              break;
                case "lexicon.search":
                case "search":     this.Lexicon = new QLexicalDomain(setting.Value); return true;
                case "lexicon.render":
                case "render":     this.Display = new QLexicalDisplay(setting.Value); return true;
                case "format":     this.Format = new QFormat(setting.Value);          break;
                case "similarity": this.Similarity = new QSimilarity(setting.Value, this);  break;
                default:           return false;
            }
            return Update();
        }
        public bool Clear(QClear clear)
        {
            switch (clear.Key)
            {
                case "span":       this.Span = new QSpan(QSpan.DEFAULT);                        break;
                case "lexicon.search":
                case "search":     this.Lexicon = new QLexicalDomain(QLexicalDomain.DEFAULT); return true;
                case "lexicon.render":
                case "render":     this.Display = new QLexicalDisplay(QLexicalDisplay.DEFAULT); return true;
                case "format":     this.Format = new QFormat(QFormat.DEFAULT);                  break;
                case "similarity": this.Similarity = new QSimilarity(QSimilarity.DEFAULT.word, QSimilarity.DEFAULT.lemma); break;

                case "all":        this.Span = new QSpan(QSpan.DEFAULT);
                                   this.Lexicon = new QLexicalDomain(QLexicalDomain.DEFAULT);
                                   this.Display = new QLexicalDisplay(QLexicalDisplay.DEFAULT);
                                   this.Format = new QFormat(QFormat.DEFAULT);
                                   this.Similarity = new QSimilarity(QSimilarity.DEFAULT.word, QSimilarity.DEFAULT.lemma); break;

                default:           return false;
            }
            return Update();
        }
        public bool Update()
        {
            if (this.BackingStore != null)
            {
                try
                {
                    using (StreamWriter sw = File.CreateText(this.BackingStore))
                    {
                        if (this.Lexicon.Value != QLexicalDomain.DEFAULT)
                            sw.WriteLine(this.Lexicon.AsYaml());
                        if (this.Display.Value != QLexicalDisplay.DEFAULT)
                            sw.WriteLine(this.Display.AsYaml());
                        if (this.Similarity.Value != QSimilarity.DEFAULT)
                            sw.WriteLine(this.Similarity.AsYaml());
                        if (this.Format.Value != QFormat.DEFAULT)
                            sw.WriteLine(this.Format.AsYaml());
                        if (this.Span.Value != QSpan.DEFAULT)
                            sw.WriteLine(this.Span.AsYaml());
                    }
                }
                catch
                {
                    return false;   // eat exception for now; now QContext yet to write error to
                }
            }
            return true;
        }
        public bool Reload()
        {
            if ((this.BackingStore != null) && File.Exists(this.BackingStore))
            {
                var lines = File.ReadLines(this.BackingStore);
                foreach (var line in lines)
                {
                    var kv = line.Split(':');

                    if (kv.Length == 2)
                    {
                        var key = kv[0].Trim();
                        var val = kv[1].Trim();

                        switch (key)
                        {
                            case "span":       this.Span = new QSpan(val);              break;
                            case "lexicon.search":
                            case "search":     this.Lexicon = new QLexicalDomain(val);      break;
                            case "lexicon.render":
                            case "render":     this.Display = new QLexicalDisplay(val); break;
                            case "format":     this.Format = new QFormat(val);          break;
                            case "similarity": this.Similarity = new QSimilarity(val);  break;
                        }
                    }
                }
            }
            return true;
        }
        public QFormat         Format     { get; set; }
        public QLexicalDomain  Lexicon    { get; set; }
        public QLexicalDisplay Display    { get; set; }
        public QSpan           Span       { get; set; }
        public QSimilarity     Similarity { get; set; }

        // Implement ISettings
        [YamlIgnore]
        public bool SearchAsAV            { get => this.Lexicon.Value == QLexicon.QLexiconVal.AV   || this.Lexicon.Value == QLexicon.QLexiconVal.BOTH; }
        [YamlIgnore]
        public bool SearchAsAVX           { get => this.Lexicon.Value == QLexicon.QLexiconVal.AVX  || this.Lexicon.Value == QLexicon.QLexiconVal.BOTH; }
        [YamlIgnore]
        public bool RenderAsAV            { get => this.Display.Value == QLexicon.QLexiconVal.AV  || this.Display.Value == QLexicon.QLexiconVal.BOTH; }
        [YamlIgnore]
        public bool RenderAsAVX           { get => this.Display.Value == QLexicon.QLexiconVal.AVX || this.Display.Value == QLexicon.QLexiconVal.BOTH; }
        [YamlIgnore]
        public int  RenderingFormat       { get => (int)this.Format.Value; }
        [YamlIgnore]
        public (byte word, byte lemma) SearchSimilarity      { get => this.Similarity.Value; }
        [YamlIgnore]
        public UInt16 SearchSpan          { get => this.Span.Value; }

        private string? BackingStore;
#pragma warning disable CS8618
        public QSettings()
        {
            this.BackingStore = null;
            this.ResetDefaults(); // silence the compiler by doing this explicitly:
        }
        public QSettings(QSettings source)
        {
            this.BackingStore = null;
            this.CopyFrom(source);
        }
        public QSettings(string yamlFile) // yaml file can be user's persistent settings file xor a macro-definition which contains QSettings at time of creation
        {
            this.BackingStore = yamlFile;
            this.ResetDefaults();
            this.Reload();
        }
        public QSettings(QSettings source, string yamlFile)
        {
            this.BackingStore = yamlFile;
            this.CopyFrom(source);
            this.Reload();
        }
        public QSettings ResetDefaults()
        {
            this.Format  = new QFormat();
            this.Lexicon = new QLexicalDomain();
            this.Display = new QLexicalDisplay();
            this.Span    = new QSpan();
            this.Similarity = new QSimilarity();
            return this;
        }
        public QSettings CopyFrom(QSettings source)
        {
            this.Format  = new QFormat(source.Format.Value);
            this.Lexicon = new QLexicalDomain(source.Lexicon.Value);
            this.Display = new QLexicalDisplay(source.Display.Value);
            this.Span    = new QSpan(source.Span.Value);
            this.Similarity = new QSimilarity(source.Similarity.Value.word, source.Similarity.Value.lemma);

            return this;
        }
        public List<string> AsYaml(bool showDefaults = true, bool showExtendedSettings = false, HashSet<string>? exclude = null, HashSet<string>? include = null)
        {
            var yaml = new List<string>();
            string val;

            StringBuilder table = new StringBuilder(1024);

            bool isHeader = true;
            foreach (string key in MarkdownRow.Keys)
            {
                if (include != null && !include.Contains(key)) // include means that if key is NOT in include list, then exclude it.
                    continue;
                if (exclude != null && exclude.Contains(key))
                    continue;

                if (isHeader)
                {
                    isHeader = false;
                }
                else if (key == QSpan.Name)
                {
                    if (showDefaults || (this.Span.Value != QSpan.DEFAULT))
                    {
                        yaml.Add(this.Span.AsYaml());
                    }
                }
                else if (key == QLexicalDomain.Name)
                {
                    if (showDefaults || (this.Lexicon.Value == QLexicalDomain.DEFAULT))
                    {
                        yaml.Add(this.Lexicon.AsYaml());
                    }
                }
                else if (key == QLexicalDisplay.Name)
                {
                    if (showDefaults || (this.Display.Value == QLexicalDisplay.DEFAULT))
                    {
                        yaml.Add(this.Display.AsYaml());
                    }
                }
                else if (key == QFormat.Name)
                {
                    if (showDefaults || (this.Format.Value == QFormat.DEFAULT))
                    {
                        yaml.Add(this.Format.AsYaml());
                    }
                }
                else if (key == QSimilarity.Name)
                {
                    if (showDefaults || (this.Similarity.Value == QSimilarity.DEFAULT))
                    {
                        yaml.Add(this.Similarity.AsYaml());
                    }
                }
                else if (showExtendedSettings && key.Equals(REVISION))
                {
                    yaml.Add(QSettings.AsYaml(key, Pinshot_RustFFI.VERSION));
                }
            }
            return yaml;
        }
        public static string AsYaml(string key, string value)
        {
            return key + ": " + value;
        }

        private static string GetKeyRepresentation(string key, string? bold)
        {
            if ((bold == null) || bold.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                return key;

            return "**" + key + "**";
        }
        public string AsMarkdown(bool showDefaults = true, bool showExtendedSettings = false, HashSet<string>? exclude = null, HashSet<string>? include = null, string? bold = null)
        {
            string val;
            List<string> rows = new();

            StringBuilder table = new StringBuilder(1024);

            string header = string.Empty;
            bool isHeader = true;
            foreach (string key in MarkdownRow.Keys)
            {
                if (include != null && !include.Contains(key)) // include means that if key is NOT in include list, then exclude it.
                    continue;
                if (exclude != null && exclude.Contains(key))
                    continue;

                if (isHeader)
                {
                    header = MarkdownRow[key];
                    isHeader = false;
                }
                else if (key == QSpan.Name)
                {
                    if (showDefaults || (this.Span.Value != QSpan.DEFAULT))
                    {
                        val = this.Span.ToString();
                        table.Append(string.Format(MarkdownRow[key], key, QSettings.GetKeyRepresentation(val, bold)));
                    }
                }
                else if (key == QLexicalDomain.Name)
                {
                    if (showDefaults || (this.Lexicon.Value == QLexicalDomain.DEFAULT))
                    {
                        val = this.Lexicon.ToString();
                        table.Append(string.Format(MarkdownRow[key], key, QSettings.GetKeyRepresentation(val, bold)));
                    }
                }
                else if (key == QLexicalDisplay.Name)
                {
                    if (showDefaults || (this.Display.Value == QLexicalDisplay.DEFAULT))
                    {
                        val = this.Display.ToString();
                        table.Append(string.Format(MarkdownRow[key], key, QSettings.GetKeyRepresentation(val, bold)));
                    }
                }
                else if (key == QFormat.Name)
                {
                    if (showDefaults || (this.Format.Value == QFormat.DEFAULT))
                    {
                        val = this.Format.ToString();
                        table.Append(string.Format(MarkdownRow[key], key, QSettings.GetKeyRepresentation(val, bold)));
                    }
                }
                else if (key == QSimilarity.Name)
                {
                    if (showDefaults || (this.Similarity.Value == QSimilarity.DEFAULT))
                    {
                        val = this.Similarity.ToString();
                        table.Append(string.Format(MarkdownRow[key], key, QSettings.GetKeyRepresentation(val, bold)));
                    }
                }
                else if (showExtendedSettings && key.Equals(REVISION))
                {
                    table.Append(string.Format(MarkdownRow[key], key, QSettings.GetKeyRepresentation(Pinshot_RustFFI.VERSION, bold)));
                    table.Append(MarkdownRow[key]);
                }
            }
            if (table.Length > 0)
            {
                table.Insert(0, header);
                return table.ToString();
            }
            return string.Empty;
        }
        public const string REVISION = "revision";
        public static Dictionary<string, string> MarkdownRow { get; private set; } = new();
        static QSettings()
        {
            QSettings.MarkdownRow.Add("header", "| Setting | Meaning | Value |\n" + "| ---------- | ------------------------------------------------------------ | ------------ |");
            QSettings.MarkdownRow.Add(QSpan.Name, "| {0}        | proximity distance limit                                     | {1}   |");
            QSettings.MarkdownRow.Add(QLexicalDomain.Name, "| {0}        | the lexicon to be used for the searching                     | {1}   |");
            QSettings.MarkdownRow.Add(QLexicalDisplay.Name, "| {0}        | the lexicon to be used for display / rendering | {1}   |");
            QSettings.MarkdownRow.Add(QFormat.Name, "| {0}        | format of results on output(e.g. for exported results)      | {1}   |");
            QSettings.MarkdownRow.Add(QSimilarity.Name, "| {0}        | fuzzy phonetics matching threshold is between 1 and 99 < br /> 0 or* none *means: do not match on phonetics(use text only) < br /> 100 or* exact*means that an *exact * phonetics match is expected | {1} |");
            QSettings.MarkdownRow.Add(REVISION, "| {0}        | revision number of the grammar. This value is read - only.     | {1}   |");
        }
    }
}