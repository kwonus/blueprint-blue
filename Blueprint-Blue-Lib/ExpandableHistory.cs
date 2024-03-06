namespace Blueprint.Blue
{
    using YamlDotNet.Serialization.NamingConventions;
    using YamlDotNet.Serialization;
    using System.Text;

    public class ExpandableHistory: ExpandableInvocation
    {
        public UInt64 Id;
        public ExpandableHistory(): base()
        {
            this.Id = 0;
        }

        public ExpandableHistory(QSelectionCriteria statement, UInt64 id, bool partial): base(statement, partial)
        {
            this.Id = id;
        }
        public ExpandableHistory(QUtilize invocation) : base(invocation)
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
                return history;
            }
            catch
            {
                ;
            }
            return new Dictionary<long, ExpandableHistory>();
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
