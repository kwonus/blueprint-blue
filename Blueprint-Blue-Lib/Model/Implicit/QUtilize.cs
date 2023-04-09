using Pinshot.PEG;

namespace Blueprint.Blue
{
    public class QUtilize : QImplicitCommand, ICommand
    {
        public string Label { get; set; }

        public override string Expand()
        {
            return this.Text;
        }
        private QUtilize(QContext env, string text, string label) : base(env, text, "invoke")
        {
            this.Label = label;
        }
        public static QUtilize? Create(QContext env, string text, Parsed[] args)
        {
            return args.Length == 1 && args[0].rule.ToLower() == "label" ? new QUtilize(env, text, args[0].text) : null;
        }
    }
}