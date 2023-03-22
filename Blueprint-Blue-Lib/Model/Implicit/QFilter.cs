using Pinshot.PEG;

namespace Blueprint.Blue
{
    public class QFilter : QImplicitCommand, ICommand
    {
        public string Scope { get; set; }

        private QFilter(QEnvironment env, string text, string scope) : base(env, text, "filter")
        {
            this.Scope = scope;
        }
        public static QFilter Create(QEnvironment env, string text, Parsed[] args)
        {
            return new QFilter(env, text, "foo");
        }
    }
}