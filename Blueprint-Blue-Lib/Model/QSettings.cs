using BlueprintBlue;
using Pinshot.Blue;
using Pinshot.PEG;
using System.Data;

namespace Blueprint.Blue
{
    public class QSettings
    {
        public bool Update()
        {
            if (this.BackingStore != null)
            {
                try
                {
                    using (StreamWriter sw = File.CreateText(this.BackingStore))
                    {
                        if (this.Domain.Value != QDomain.DEFAULT)
                            sw.WriteLine("domain: " + this.Domain.ToString());
                        if (this.Exact.Value != QExact.DEFAULT)
                            sw.WriteLine("exact:  " + this.Exact.ToString());
                        if (this.Format.Value != QFormat.DEFAULT)
                            sw.WriteLine("format: " + this.Format.ToString());
                        if (this.Span.Value != QSpan.DEFAULT)
                            sw.WriteLine("span:   " + this.Span.ToString());
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

                        switch (trimmed)
                        {
                            case "span": this.Span = new QSpan(kv[1]); break;
                            case "domain": this.Domain = new QDomain(kv[1]); break;
                            case "exact": this.Exact = new QExact(kv[1]); break;
                            case "format": this.Format = new QFormat(kv[1]); break;
                        }
                    }
                }
            }
            return true;
        }
        public QFormat Format { get; set; }
        public QDomain Domain { get; set; }
        public QSpan Span     { get; set; }
        public QExact Exact   { get; set; }

        private string? BackingStore;

        public QSettings()
        {
            this.BackingStore = null;
            this.ResetDefaults();
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
            this.Format = new QFormat();
            this.Domain = new QDomain();
            this.Span   = new QSpan();
            this.Exact  = new QExact();
            return this;
        }
        public QSettings CopyFrom(QSettings source)
        {
            this.Format = new QFormat(source.Format.Value);
            this.Domain = new QDomain(source.Domain.Value);
            this.Span   = new QSpan(source.Span.Value);
            this.Exact  = new QExact(source.Exact.Value);

            return this;
        }
    }
}