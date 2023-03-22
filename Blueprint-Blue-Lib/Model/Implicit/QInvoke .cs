using Pinshot.PEG;

namespace Blueprint.Blue
{
    public class QInvoke : QImplicitCommand, ICommand
    {
        public string Label { get; set; }

        private QInvoke(QEnvironment env, string text, string label) : base(env, text, "invoke")
        {
            this.Label = label;
        }
        public static QInvoke Create(QEnvironment env, string text, Parsed[] args)
        {
            return new QInvoke(env, text, "foo");
        }
    }
}