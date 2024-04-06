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
        public static Dictionary<long, ExpandableHistory> HistoryDeserializer(string yamlpath)
        {
            try
            {
                Dictionary<long, ExpandableHistory> history;

                using (StreamReader sr = new StreamReader(yamlpath))
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
            return new();
        }
        public static ExpandableHistory? MacroDeserializer(string yamlpath)
        {
            try
            {
                ExpandableHistory macro;

                using (StreamReader sr = new StreamReader(yamlpath))
                {
                    string text = sr.ReadToEnd();
                    var input = new StringReader(text);
                    var deserializer = new DeserializerBuilder().WithNamingConvention(PascalCaseNamingConvention.Instance).Build();
                    macro = deserializer.Deserialize<ExpandableHistory>(input);
                }
                return macro;
            }
            catch
            {
                ;
            }
            return null;
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
