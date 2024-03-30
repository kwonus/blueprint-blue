namespace Blueprint.Blue
{
    using YamlDotNet.Serialization.NamingConventions;
    using YamlDotNet.Serialization;
    using System.Text;
    using System.IO;

    public class ExpandableHistory: ExpandableInvocation
    {
        public UInt64 Id;
        public ExpandableHistory(): base()
        {
            this.Id = 0;
        }

        public ExpandableHistory(string rawText, QSelectionCriteria statement, UInt64 id) : base(rawText, statement)
        {
            this.Id = id;
        }
        public ExpandableHistory(string rawText, QUtilize invocation) : base(rawText, invocation)
        {
            this.Id = invocation.Id ?? 0;
        }
        public static Dictionary<long, ExpandableHistory> YamlDeserializer(string yaml)
        {
            try
            {
                Dictionary<long, ExpandableHistory> history;

                using (StreamReader sr = new StreamReader(yaml))
                {
                    string text = sr.ReadToEnd();
                    var input = new StringReader(text);
                    var deserializer = new DeserializerBuilder().WithNamingConvention(PascalCaseNamingConvention.Instance).Build();
                    history = deserializer.Deserialize<Dictionary<long, ExpandableHistory>>(input);
                }
                return (history != null) ? history : new();
            }
            catch
            {
                ;
            }
            return new();
        }
        public new string AsMarkdown()
        {
            StringBuilder markdown = new(1024);
            markdown.AppendLine("####" + this.Id.ToString());
            markdown.Append(base.AsMarkdown());
            return markdown.ToString();
        }
    }
}
