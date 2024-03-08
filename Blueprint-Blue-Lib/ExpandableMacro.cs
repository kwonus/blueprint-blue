namespace Blueprint.Blue
{
    using YamlDotNet.Serialization.NamingConventions;
    using YamlDotNet.Serialization;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.IO;

    public class ExpandableMacro: ExpandableInvocation
    {
        public string Label { get; private set; }
        public ExpandableMacro() : base(MacroComponents.Ignore)
        {
            this.Label = string.Empty;
        }

        public ExpandableMacro(string rawText, QSelectionCriteria statement, string label, MacroComponents parts) : base(rawText, statement, parts)
        {
            this.Label = label;
        }
        public ExpandableMacro(string rawText, QUtilize invocation, MacroComponents parts) : base(rawText, invocation, parts)
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
