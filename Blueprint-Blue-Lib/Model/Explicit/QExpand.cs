namespace Blueprint.Blue
{
    using Pinshot.PEG;

    public class QExpand : QExplicitCommand, ICommand
    {
        public string Label { get; set; }
        public QExpand(QContext env, string text, Parsed[] args) : base(env, text, "expand")
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