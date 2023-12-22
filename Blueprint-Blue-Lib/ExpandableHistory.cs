using Blueprint.Blue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

namespace BlueprintBlue
{
    public class ExpandableHistory: ExpandableInvocation
    {
        public UInt64 Id;
        public ExpandableHistory(): base()
        {
            this.Id = 0;
        }

        public ExpandableHistory(QCommandSegment statement, UInt64 id): base(statement)
        {
            this.Id = id;
        }
        public ExpandableHistory(QInvoke invocation) : base(invocation)
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
    }
}
