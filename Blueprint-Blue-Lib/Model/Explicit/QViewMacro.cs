namespace Blueprint.Blue
{
    using Pinshot.PEG;

    public class QViewMacro : QView, ICommand     // QReview object is the @view command for macros
    {
        public string Label { get; private set; } // macro

        public QViewMacro(QContext env, string text, Parsed[] args) : base(env, text)
        {
            this.Label = string.Empty;
            this.ParseLabel(args);
        }
        private void ParseLabel(Parsed[] args)
        {
            foreach (Parsed arg in args)
            {
                if (arg.rule == "tag")
                {
                    this.Label = arg.text;
                    break;
                }
            }
        }
        public override (bool ok, string message) Execute()
        {
            return (false, "Operation has not been implemented yet.");
        }
    }
}