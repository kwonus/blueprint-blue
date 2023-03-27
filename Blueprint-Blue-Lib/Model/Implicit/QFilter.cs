using Pinshot.PEG;

namespace Blueprint.Blue
{
    public class QFilter : QImplicitCommand, ICommand
    {
        public string Rule { get; private set; }
        public string Filter { get; private set; }

        private QFilter(QEnvironment env, string text, string rule, string filter) : base(env, text, "filter")
        {
            this.Rule = rule;
            this.Filter = filter;
        }
        public static QFilter? Create(QEnvironment env, string text, Parsed[] args)
        {
            return args.Length == 1 ? new QFilter(env, text, args[0].rule, args[0].text) : null;
        }
    }
}