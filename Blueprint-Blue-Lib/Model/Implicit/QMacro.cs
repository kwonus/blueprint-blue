using Pinshot.PEG;

namespace Blueprint.Blue
{
    public class QMacro : QImplicitCommand, ICommand
    {
        public string Label { get; set; }

        private QMacro(QContext env, string text, string label) : base(env, text, "macro")
        {
            this.Label = label;
        }
        public static QMacro Create(QContext env, string text, Parsed[] args)
        {
            return new QMacro(env, text, "foo");
        }
    }
}