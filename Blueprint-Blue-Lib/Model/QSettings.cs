namespace Blueprint.Blue
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using BlueprintBlue.Model.Implicit;
    using Pinshot.Blue;

    public class QSettings
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
                case "lexicon":    return this.Lexicon.ToString();
                case "display":    return this.Display.ToString();
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
                case "lexicon":    this.Lexicon = new QLexicalDomain(assignment.Value);  return true;
                case "display":    this.Display = new QLexicalDisplay(assignment.Value); return true;
                case "format":     this.Format = new QFormat(assignment.Value);          return true;
                case "similarity": this.Similarity = new QSimilarity(assignment.Value);  return true;
            }
            return false;
        }
        public bool Set(QSet setting)
        {
            string key = (setting.Key.Length >= 1 && setting.Key[0] == '%') ? setting.Key.Substring(1) : setting.Key;
            switch (key)
            {
                case "span":       this.Span = new QSpan(setting.Value);              break;
                case "lexicon":    this.Lexicon = new QLexicalDomain(setting.Value);  break;
                case "display":    this.Display = new QLexicalDisplay(setting.Value); break;
                case "format":     this.Format = new QFormat(setting.Value);          break;
                case "similarity": this.Similarity = new QSimilarity(setting.Value);  break;
                default:           return false;
            }
            return Update();
        }
        public bool Clear(QClear clear)
        {
            string key = (clear.Key.Length >= 1 && clear.Key[0] == '%') ? clear.Key.Substring(1) : clear.Key;
            switch (key)
            {
                case "span":       this.Span = new QSpan(QSpan.DEFAULT);                        break;
                case "lexicon":    this.Lexicon = new QLexicalDomain(QLexicalDomain.DEFAULT);   break;
                case "display":    this.Display = new QLexicalDisplay(QLexicalDisplay.DEFAULT); break;
                case "format":     this.Format = new QFormat(QFormat.DEFAULT);                  break;
                case "similarity": this.Similarity = new QSimilarity(QSimilarity.DEFAULT);      break;

                case "all":        this.Span = new QSpan(QSpan.DEFAULT);
                                    this.Lexicon = new QLexicalDomain(QLexicalDomain.DEFAULT);
                                    this.Display = new QLexicalDisplay(QLexicalDisplay.DEFAULT);
                                    this.Format = new QFormat(QFormat.DEFAULT);
                                    this.Similarity = new QSimilarity(QSimilarity.DEFAULT);      break;

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
                            case "lexicon":    this.Lexicon = new QLexicalDomain(val);  break;
                            case "display":    this.Display = new QLexicalDisplay(val); break;
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
            this.Similarity = new QSimilarity(source.Similarity.Value);

            return this;
        }
        public List<string> AsYaml()
        {
            var yaml = new List<string>();
            yaml.Add(this.Span.AsYaml());
            yaml.Add(this.Similarity.AsYaml());
            yaml.Add(this.Format.AsYaml());
            yaml.Add(this.Lexicon.AsYaml());
            yaml.Add(this.Display.AsYaml());
            return yaml;
        }
    }
}