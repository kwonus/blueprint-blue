namespace Blueprint.Blue
{
    using Pinshot.PEG;
    using System.Collections.Generic;
    using YamlDotNet.Serialization;

    public class QFilter : QImplicitCommand, ICommand
    {
        public string Rule { get; private set; }
        public string Filter { get; private set; }

        public override string Expand()
        {
            return this.Text;
        }

        private QFilter(QContext env, string text, string rule, string filter) : base(env, text, "filter")
        {
            this.Rule = rule;
            this.Filter = filter;
        }
        public static QFilter? Create(QContext env, string text, Parsed[] args)
        {
            return args.Length == 1 ? new QFilter(env, text, args[0].rule, args[0].text) : null;
        }
        public override List<string> AsYaml()
        {
            return ICommand.YamlSerializer(this);
        }
    }
}