namespace Blueprint.Blue
{
    using YamlDotNet.Serialization.NamingConventions;
    using YamlDotNet.Serialization;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;

    public class ExpandableMacro: ExpandableInvocation
    {
        public string Label { get; private set; }
        public ExpandableMacro() : base()
        {
            this.Label = string.Empty;
        }

        public ExpandableMacro(QCommandSegment statement, string label, bool partial) : base(statement, partial)
        {
            this.Label = label;
        }
        public ExpandableMacro(QUtilizize invocation) : base(invocation)
        {
            this.Label = invocation.Label ?? string.Empty;
        }
        public static Dictionary<string, ExpandableMacro> YamlDeserializer(string folder)
        {
            var macros = new Dictionary<string, ExpandableMacro>();
            if (Directory.Exists(folder))
            {
                foreach (var yaml in Directory.EnumerateFiles(folder, "*.yaml"))
                {
                    using (StreamReader sr = new StreamReader(yaml))
                    {
                        try
                        {
                            string text = sr.ReadToEnd();
                            var input = new StringReader(text);
                            var deserializer = new DeserializerBuilder().WithNamingConvention(PascalCaseNamingConvention.Instance).Build();
                            var macro = deserializer.Deserialize<ExpandableMacro>(input);
                            macros.Add(macro.Label, macro);
                        }
                        catch { }
                    }
                }
            }
            return macros;
        }
        public new string AsMarkdown()
        {
            StringBuilder markdown = new(1024);
            markdown.AppendLine("####" + this.Label);
            markdown.Append(base.AsMarkdown());
            return markdown.ToString();
        }
    }
}
