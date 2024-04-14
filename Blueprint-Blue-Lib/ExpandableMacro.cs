namespace Blueprint.Blue
{
    using YamlDotNet.Serialization.NamingConventions;
    using YamlDotNet.Serialization;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.IO;
    using YamlDotNet.Core;
    using Blueprint.Model.Implicit;

    public class ExpandableMacro: ExpandableInvocation
    {
        private string Label;
        public override string Tag
        {
            get => this.Label;
            protected set => this.Label = value;
        }
        public ExpandableMacro() : base()
        {
            this.Label = string.Empty;
        }

        public ExpandableMacro(string rawText, QSelectionCriteria statement, string label) : base(rawText, statement)
        {
            this.Label = label;
        }
        public ExpandableMacro(string rawText, QUtilize invocation) : base(rawText, invocation)
        {
            this.Label = invocation.Tag ?? string.Empty;
        }
        public static ExpandableMacro? Deserialize(string tag)
        {
            if (Directory.Exists(QContext.MacroPath))
            {
                if (Directory.Exists(QContext.MacroPath))
                {
                    var yaml = Path.Combine(QContext.MacroPath, tag + ".yaml");

                    if (File.Exists(yaml))
                    {
                        using (StreamReader sr = new StreamReader(yaml))
                        {
                            try
                            {
                                string text = sr.ReadToEnd();
                                var input = new StringReader(text);
                                var deserializer = new DeserializerBuilder()/*.WithNamingConvention(PascalCaseNamingConvention.Instance)*/.Build();
                                var macro = deserializer.Deserialize<ExpandableMacro>(input);
                                return macro;
                            }
                            catch { }
                        }
                    }
                }
            }
            return null;
        }
        public bool Serialize()
        {
            if (!string.IsNullOrEmpty(this.Tag))
            {
                string yaml = Path.Combine(QContext.MacroPath, this.Tag + ".yaml");
                try
                {
                    YamlDotNet.Serialization.Serializer serializer = new();

                    using (var stream = new FileStream(yaml, FileMode.Create))
                    {
                        // StreamWriter object that writes UTF-8 encoded text
                        using (var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: false))
                        {
                            serializer.Serialize(writer, this);
                            writer.Flush(); // Make sure all data is written to the MemoryStream.
                        }
                        return true;
                    }
                }
                catch
                {
                    ;
                }
            }
            return false;
        }
    }
}
