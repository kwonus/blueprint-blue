namespace Blueprint.Blue
{
    using Pinshot.PEG;
    using System.Collections.Generic;
    using System.Linq;
    using System;
    using System.Text;
    using YamlDotNet.Serialization;

    public class QLimit : QImplicitCommand, ICommand
    {
        public override string Expand()
        {
            return this.Text;
        }
        public List<uint> Fields { get; private set; }

        private QLimit(QContext env, string text, IEnumerable<uint> fields) : base(env, text, "limit")
        {
            this.Fields = new();

            foreach (var field in fields)
                this.Fields.Add(field);
        }
        public static QLimit? Create(QContext env, string text, Parsed[] args)
        {
            try
            {
                var fields = from field in args where field.rule.Equals("DIGITS", StringComparison.InvariantCultureIgnoreCase) select uint.Parse(field.text);
                return new QLimit(env, text, (fields));
            }
            catch
            {
                ;
            }
            return null;
        }
        public override List<string> AsYaml()
        {
            return ICommand.YamlSerializer(this);
        }
    }
}