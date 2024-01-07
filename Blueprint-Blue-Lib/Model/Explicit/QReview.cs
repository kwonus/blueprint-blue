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
            if (args != null && args.Length >= 1)
            {
                this.Label = args[0].text;
                this.Arguments = new string[args.Length-1];

                for (int i = 0; i < this.Arguments.Length; i++)
                {
                    this.Arguments[i] = args[i+1].text;
                }
            }
            else
            {
                this.Label = string.Empty;
                this.Arguments = new string[0];
            }
        }
        public override (bool ok, string message) Execute()
        {
            return (false, "Operation has not been implemented yet.");
        }
    }
}