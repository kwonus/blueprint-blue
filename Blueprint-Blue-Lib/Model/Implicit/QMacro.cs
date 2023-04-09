using Pinshot.PEG;

namespace Blueprint.Blue
{
    public class QMacro : QImplicitCommand, ICommand
    {
        public string Label { get; set; }

        public override string Expand()
        {
            return this.Text;
        }
        private QMacro(QContext env, string text, string label) : base(env, text, "macro")
        {
            this.Label = label;
        }
        public static QMacro? Create(QContext env, string text, Parsed[] args)
        {
            if (args.Length == 1 && args[0].rule == "label")
            { 
                return new QMacro(env, text, args[0].text);
            }
            return null;
        }
    }
}