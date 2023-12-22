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
    public class ExpandableMacro: ExpandableInvocation
    {
        public string Label;
        public ExpandableMacro() : base()
        {
            this.Label = string.Empty;
        }

        public ExpandableMacro(QCommandSegment statement, string label) : base(statement)
        {
            this.Label = label;
        }
        public ExpandableMacro(QInvoke invocation) : base(invocation)
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
    }
}
