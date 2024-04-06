namespace Blueprint.Blue
{
    using Pinshot.PEG;

    public class QViewMacro : QView, ICommand     // QReview object is the @view command for macros
    {
        public string? Label { get; private set; } // macro
        public string? Wildcard { get; private set; } // macro

        public QViewMacro(QContext env, string text, Parsed[] args) : base(env, text)
        {
            this.Label = null;
            this.Wildcard = null;
            this.ParseDateRange(args);
            this.ParseLabel(args);
            if (this.Label == null)
                this.ParseWildcard(args);
        }
        protected void ParseWildcard(Parsed[] args)
        {
            foreach (Parsed arg in args)
            {
                if (arg.rule == "wildcard")
                {
                    this.Wildcard = arg.text;
                    break;
                }
            }
        }
        protected void ParseLabel(Parsed[] args)
        {
            foreach (Parsed arg in args)
            {
                if (arg.rule == "tag")
                {
                    this.Wildcard = arg.text;
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