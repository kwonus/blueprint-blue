namespace Blueprint.Blue
{
    using AVSearch.Interfaces;
    using Pinshot.PEG;

    public class QBulkMacros : QBulk, ICommand     // QReview object is the @view command for macros
    {
        public string? Label { get; private set; } // macro
        public string? Wildcard { get; private set; } // macro

        public QBulkMacros(QContext env, string text, Parsed[] args, BulkAction action) : base(env, text, "macros", action)
        {
            this.Label = null;
            this.Wildcard = null;
            this.ParseDateRange(args);
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
        public override (bool ok, string message) Execute()
        {
            return (false, "Operation has not been implemented yet.");
        }
    }
}