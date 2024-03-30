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
    using System.Reflection;
    using System.Runtime.InteropServices.Marshalling;

    public class QSettings: ISettings
    {
        public string GetAll()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("span:             "); builder.AppendLine(this.Span.ToString());
            builder.Append("format:           "); builder.AppendLine(this.Format.ToString());
            builder.Append("lexicon.search:   "); builder.AppendLine(this.Lexicon.Domain.ToString());
            builder.Append("lexicon.render:   "); builder.AppendLine(this.Lexicon.Render.ToString());
            builder.Append("similarity.word:  "); builder.AppendLine(this.Similarity.Word.ToString());
            builder.Append("similarity.lemma: "); builder.AppendLine(this.Similarity.Lemma.ToString());
            builder.Append("version:    "); builder.Append(Pinshot_RustFFI.VERSION);

            return builder.ToString();
        }
        public string Get(QGet setting)
        {
            string key = (setting.Key.Length >= 1 && setting.Key[0] == '@') ? setting.Key.Substring(1) : setting.Key;
            switch (key)
            {
                case "span":             return this.Span.ToString();
                case "lexicon":          return this.Lexicon.ToString();
                case "lexicon.search":
                case "search.lexicon":
                case "search":           return this.Lexicon.Domain.ToString();
                case "lexicon.render":
                case "render.lexicon":
                case "render":           return this.Lexicon.Render.ToString();
                case "format":           return this.Format.ToString();
                case "similarity":       return this.Similarity.ToString();
                case "similarity.word":
                case "word":             return this.Similarity.Word.ToString();
                case "similarity.lemma":
                case "lemma":            return this.Similarity.Lemma.ToString();
                case "version":        
                case "grammar.revision":        
                case "revision":         return Pinshot_RustFFI.VERSION;

                case "all":              return this.GetAll();          
            }

            return this.GetAll();
        }
        public bool Assign(QAssign assignment)
        {
            string key = (assignment.Key.Length >= 1 && assignment.Key[0] == '@') ? assignment.Key.Substring(1) : assignment.Key;
            switch (key)
            {
                case "span":             this.Span    = new QSpan(assignment.Value);                                                               return true;  
                case "lexicon":          this.Lexicon = new QLexicon(assignment.Value);                                                            return true;
                case "lexicon.search":
                case "search.lexicon":
                case "search":           this.Lexicon = new QLexicon(search: assignment.Value, render:this.Lexicon.Render.Value.ToString());       return true;
                case "lexicon.render":
                case "render.lexicon":
                case "render":           this.Lexicon = new QLexicon(search:this.Lexicon.Domain.Value.ToString(), render:assignment.Value);        return true;
                case "format":           this.Format = new QFormat(assignment.Value);                                                              return true;
                case "similarity":       this.Similarity = new QSimilarity(assignment.Value);                                                      return true;
                case "similarity.word":
                case "word":             this.Similarity = new QSimilarity(sword:assignment.Value, slemma:this.Similarity.Lemma.Value.ToString()); return true;
                case "similarity.lemma":
                case "lemma":            this.Similarity = new QSimilarity(sword:this.Similarity.Lemma.Value.ToString(), slemma:assignment.Value); return true;
            }
            return false;
        }
        public bool Set(QSet setting)
        {
            switch (setting.Key)
            {
                case "span":             this.Span    = new QSpan(setting.Value);                                                                break;  
                case "lexicon":          this.Lexicon = new QLexicon(setting.Value);                                                             break;
                case "lexicon.search":
                case "search.lexicon":
                case "search":           this.Lexicon = new QLexicon(search: setting.Value, render:this.Lexicon.Render.Value.ToString());        break;
                case "lexicon.render":
                case "render.lexicon":
                case "render":           this.Lexicon = new QLexicon(search:this.Lexicon.Domain.Value.ToString(), render: setting.Value);        break;
                case "format":           this.Format = new QFormat(setting.Value);                                                               break;
                case "similarity":       this.Similarity = new QSimilarity(setting.Value);                                                       break;
                case "similarity.word":
                case "word":             this.Similarity = new QSimilarity(sword: setting.Value, slemma:this.Similarity.Lemma.Value.ToString()); break;
                case "similarity.lemma":
                case "lemma":            this.Similarity = new QSimilarity(sword:this.Similarity.Lemma.Value.ToString(), slemma: setting.Value); break;
                default:                 return false;
            }
            return Update();
        }
        public bool Clear(QClear clear)
        {
            switch (clear.Key)
            {
                case "span":             this.Span    = new QSpan();                                                                    break;  
                case "lexicon":          this.Lexicon = new QLexicon();                                                                 break;
                case "lexicon.search":
                case "search.lexicon":
                case "search":           this.Lexicon = new QLexicon(search:QLexicalDomain.DEFAULT, render:this.Lexicon.Render.Value);  break;
                case "lexicon.render":
                case "render.lexicon":
                case "render":           this.Lexicon = new QLexicon(search:this.Lexicon.Domain.Value, render:QLexicalDisplay.DEFAULT); break;
                case "format":           this.Format = new QFormat();                                                                   break;
                case "similarity":       this.Similarity = new QSimilarity();                                                           break;
                case "similarity.word":
                case "word":             this.Similarity = new QSimilarity(sword:QSimilarityWord.DEFAULT.ToString(), slemma:this.Similarity.Lemma.Value.ToString()); return true;
                case "similarity.lemma":
                case "lemma":            this.Similarity = new QSimilarity(sword:this.Similarity.Word.Value.ToString(), slemma:QSimilarityLemma.DEFAULT.ToString()); return true;
                default:                 return false;
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
                        sw.WriteLine(this.Lexicon.Domain.AsYaml());
                        sw.WriteLine(this.Lexicon.Render.AsYaml());
                        sw.WriteLine(this.Similarity.Word.AsYaml());
                        sw.WriteLine(this.Similarity.Lemma.AsYaml());
                        sw.WriteLine(this.Format.AsYaml());
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
                string  search = QLexicalDomain.DEFAULT.ToString();
                string  render = QLexicalDisplay.DEFAULT.ToString();
                string  lemma  = QSimilarityLemma.DEFAULT.ToString();
                string? word   = QSimilarityWord.DEFAULT.ToString();
                string? format = null;
                string? span = null;

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
                            case "span":            span = val;   break;
                            case "lexicon.search":
                            case "search.lexicon":
                            case "search":          search = val; break;
                            case "lexicon.render":
                            case "render.lexicon":
                            case "render":          render = val; break;
                            case "format":          format = val; break;
                            case "similarity.word":
                            case "word":            word = val;   break;
                            case "similarity.lemma":
                            case "lemma":           lemma = val;  break;
                        }
                    }
                }
                this.Lexicon = new QLexicon(search: search, render: render);
                this.Similarity = new QSimilarity(sword: word, slemma: lemma);

                if (string.IsNullOrWhiteSpace(format))
                    this.Format = new();
                else
                    this.Format = new(format);

                if (string.IsNullOrWhiteSpace(span))
                    this.Span = new();
                else
                    this.Span = new(span);

                return true;
            }
            return false;
        }
        public QFormat         Format     { get; set; }
        public QLexicon        Lexicon    { get; set; }
        public QSpan           Span       { get; set; }
        public QSimilarity     Similarity { get; set; }

        // Implement ISettings
        [YamlIgnore]
        public bool SearchAsAV            { get => this.Lexicon.Domain.Value == QLexicon.QLexiconVal.AV   || this.Lexicon.Domain.Value == QLexicon.QLexiconVal.BOTH; }
        [YamlIgnore]
        public bool SearchAsAVX           { get => this.Lexicon.Domain.Value == QLexicon.QLexiconVal.AVX  || this.Lexicon.Domain.Value == QLexicon.QLexiconVal.BOTH; }
        [YamlIgnore]
        public bool RenderAsAV            { get => this.Lexicon.Render.Value == QLexicon.QLexiconVal.AV  || this.Lexicon.Render.Value == QLexicon.QLexiconVal.BOTH; }
        [YamlIgnore]
        public bool RenderAsAVX           { get => this.Lexicon.Render.Value == QLexicon.QLexiconVal.AVX || this.Lexicon.Render.Value == QLexicon.QLexiconVal.BOTH; }
        [YamlIgnore]
        public int  RenderingFormat       { get => (int)this.Format.Value; }
        [YamlIgnore]
        public (byte word, byte lemma) SearchSimilarity      { get => (this.Similarity.Word.Value, this.Similarity.Lemma.Value); }
        [YamlIgnore]
        public UInt16 SearchSpan          { get => this.Span.Value; }

        private string? BackingStore;
#pragma warning disable CS8618
        public QSettings()
        {
            this.BackingStore = null;
            this.ResetDefaults(); // silence the compiler by doing this explicitly:
        }
        public static string ConvertInternalKeyToFriendlyName(string key)
        {
            switch (key)
            {
                case "span":  return "search.span";

                case "lexicon.search":
                case "search": return "search.lexicon";

                case "lexicon.render":
                case "render": return "render.lexicon";

                case "format": return "render.format";

                case "similarity.word":
                case "word": return "word.similarity";

                case "similarity.lemma":
                case "lemma": return "lemma.similarity";

                case "version":
                case "revision": return "grammar.revision";
            }
            return key;
        }
        public QSettings(Dictionary<string, string> map): this()
        {
            string  search = QLexicalDomain.DEFAULT.ToString();
            string  render = QLexicalDisplay.DEFAULT.ToString();
            string  lemma  = QSimilarityLemma.DEFAULT.ToString();
            string? word   = QSimilarityWord.DEFAULT.ToString();
            string? format = null;
            string? span = null;

            foreach (var kv in map)
            {
                switch (kv.Key)
                {
                    case "search.span":     // dictionary/map form
                    case "span":            span = kv.Value;   break;

                    case "search.lexicon":  // dictionary/map form
                    case "lexicon.search":
                    case "search":          search = kv.Value; break;

                    case "render.lexicon":  // dictionary/map form
                    case "lexicon.render":
                    case "render":          render = kv.Value; break;

                    case "render.format":   // dictionary/map form
                    case "format":          format = kv.Value; break;

                    case "word.similarity": // dictionary/map form
                    case "similarity.word":
                    case "word":            word = kv.Value;   break;

                    case "lemma.similarity":// dictionary/map form
                    case "similarity.lemma":
                    case "lemma":           lemma = kv.Value;  break;
                }
            }
            this.Lexicon = new QLexicon(search: search, render: render);
            this.Similarity = new QSimilarity(sword: word, slemma: lemma);

            if (string.IsNullOrWhiteSpace(format))
                this.Format = new();
            else
                this.Format = new(format);

            if (string.IsNullOrWhiteSpace(span))
                this.Span = new();
            else
                this.Span = new(span);
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
            this.Lexicon = new QLexicon();
            this.Span    = new QSpan();
            this.Similarity = new QSimilarity();
            return this;
        }
        public QSettings CopyFrom(QSettings source)
        {
            this.Format  = new QFormat(source.Format.Value);
            this.Lexicon = new QLexicon(search:source.Lexicon.Domain.Value, render:source.Lexicon.Render.Value);
            this.Span    = new QSpan(source.Span.Value);
            this.Similarity = QSimilarity.Create(word:source.Similarity.Word, lemma:source.Similarity.Lemma);

            return this;
        }
        public Dictionary<string, string> AsMap(bool showDefaults = true, bool showExtendedSettings = true)
        {
            var map = new Dictionary<string, string>();
            string val;

            bool isHeader = true;
            foreach (string key in MarkdownRow.Keys)
            {
                string friendlyName = ConvertInternalKeyToFriendlyName(key);
                if (isHeader)
                {
                    isHeader = false;
                }
                else if (key == QSpan.Name)
                {
                    if (showDefaults || (this.Span.Value != QSpan.DEFAULT))
                    {
                        map[friendlyName] = this.Span.Value.ToString();
                    }
                }
                else if (key == QLexicalDomain.Name)
                {
                    if (showDefaults || (this.Lexicon.Domain.Value == QLexicalDomain.DEFAULT))
                    {
                        map[friendlyName] = this.Lexicon.Domain.Value.ToString();
                    }
                }
                else if (key == QLexicalDisplay.Name)
                {
                    if (showDefaults || (this.Lexicon.Render.Value == QLexicalDisplay.DEFAULT))
                    {
                        map[friendlyName] = this.Lexicon.Domain.Value.ToString();
                    }
                }
                else if (key == QFormat.Name)
                {
                    if (showDefaults || (this.Format.Value == QFormat.DEFAULT))
                    {
                        map[friendlyName] = this.Format.Value.ToString();
                    }
                }
                else if (key == QSimilarityWord.Name)
                {
                    if (showDefaults || (this.Similarity.Word.Value == QSimilarityWord.DEFAULT))
                    {
                        map[friendlyName] = this.Similarity.Word.Value.ToString();
                    }
                }
                else if (key == QSimilarityLemma.Name)
                {
                    if (showDefaults || (this.Similarity.Lemma.Value == QSimilarityLemma.DEFAULT))
                    {
                        map[friendlyName] = this.Similarity.Lemma.Value.ToString();
                    }
                }
                else if (showExtendedSettings && key.Equals(REVISION))
                {
                    map[friendlyName] = Pinshot_RustFFI.VERSION;
                }
            }
            return map;
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
                    if (showDefaults || (this.Lexicon.Domain.Value == QLexicalDomain.DEFAULT))
                    {
                        yaml.Add(this.Lexicon.Domain.AsYaml());
                    }
                }
                else if (key == QLexicalDisplay.Name)
                {
                    if (showDefaults || (this.Lexicon.Render.Value == QLexicalDisplay.DEFAULT))
                    {
                        yaml.Add(this.Lexicon.Render.AsYaml());
                    }
                }
                else if (key == QFormat.Name)
                {
                    if (showDefaults || (this.Format.Value == QFormat.DEFAULT))
                    {
                        yaml.Add(this.Format.AsYaml());
                    }
                }
                else if (key == QSimilarityWord.Name)
                {
                    if (showDefaults || (this.Similarity.Word.Value == QSimilarityWord.DEFAULT))
                    {
                        yaml.Add(this.Similarity.AsYaml());
                    }
                }
                else if (key == QSimilarityLemma.Name)
                {
                    if (showDefaults || (this.Similarity.Lemma.Value == QSimilarityLemma.DEFAULT))
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
                    if (showDefaults || (this.Lexicon.Domain.Value == QLexicalDomain.DEFAULT))
                    {
                        val = this.Lexicon.Domain.ToString();
                        table.Append(string.Format(MarkdownRow[key], key, QSettings.GetKeyRepresentation(val, bold)));
                    }
                }
                else if (key == QLexicalDisplay.Name)
                {
                    if (showDefaults || (this.Lexicon.Render.Value == QLexicalDisplay.DEFAULT))
                    {
                        val = this.Lexicon.Render.ToString();
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
                else if (key == QSimilarityWord.Name)
                {
                    if (showDefaults || (this.Similarity.Word.Value == QSimilarityWord.DEFAULT))
                    {
                        val = this.Similarity.ToString();
                        table.Append(string.Format(MarkdownRow[key], key, QSettings.GetKeyRepresentation(val, bold)));
                    }
                }
                else if (key == QSimilarityLemma.Name)
                {
                    if (showDefaults || (this.Similarity.Lemma.Value == QSimilarityLemma.DEFAULT))
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
            QSettings.MarkdownRow.Add(QSimilarityWord.Name, "| {0}        | fuzzy phonetics matching threshold is between 1 and 99 < br /> 0 or* none *means: do not match on phonetics(use text only) < br /> 100 or* exact*means that an *exact * phonetics match is expected | {1} |");
            QSettings.MarkdownRow.Add(QSimilarityLemma.Name, "| {0}        | fuzzy phonetics matching threshold is between 1 and 99 < br /> 0 or* none *means: do not match on phonetics(use text only) < br /> 100 or* exact*means that an *exact * phonetics match is expected | {1} |");
            QSettings.MarkdownRow.Add(REVISION, "| {0}        | revision number of the grammar. This value is read - only.     | {1}   |");
        }
    }
}