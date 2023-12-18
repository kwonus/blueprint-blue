namespace Blueprint.Blue
{
    using System.Collections.Generic;
    using System.IO;
    using BlueprintBlue.Model.Implicit;

    public class QSettings
    {
        public bool Assign(QAssign assignment)
        {
            switch (assignment.Key)
            {
                case "span":       this.Span = new QSpan(assignment.Value);              break;
                case "lexicon":    this.Lexicon = new QLexicalDomain(assignment.Value);  break;
                case "display":    this.Display = new QLexicalDisplay(assignment.Value); break;
                case "format":     this.Format = new QFormat(assignment.Value);          break;
                case "similarity": this.Similarity = new QSimilarity(assignment.Value);  break;
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
                        var trimmed = kv[0].Trim();
                        var parts = trimmed.Split('.');

                        switch (parts[0])
                        {
                            case "span":       this.Span = new QSpan(trimmed);              break;
                            case "lexicon":    this.Lexicon = new QLexicalDomain(trimmed);  break;
                            case "display":    this.Display = new QLexicalDisplay(trimmed); break;
                            case "format":     this.Format = new QFormat(kv[1]);            break;
                            case "similarity": this.Similarity = new QSimilarity(trimmed);  break;
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