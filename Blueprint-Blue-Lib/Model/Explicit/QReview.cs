namespace Blueprint.Blue
{
    using Pinshot.PEG;

    public class QReview : QExplicitCommand, ICommand
    {
        public string Label { get; private set; }
        public DateTime? Since { get; private set; }
        public DateTime? Until { get; private set; }
        public string[] Arguments { get; private set; }

        public QReview(QContext env, string text, Parsed[] args) : base(env, text, "review")
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                this.Label = string.Empty;
                return;
            }
            this.Label = args != null && args.Length == 1 ? args[0].text : string.Empty;
        }
        public override (bool ok, string message) Execute()
        {
            return (false, "Operation has not been implemented yet.");
        }
    }
}