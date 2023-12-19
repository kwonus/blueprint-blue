namespace Blueprint.Blue
{
    using Pinshot.PEG;
    public class QReview : QExplicitCommand, ICommand
    {
        uint? MaxCount { get; set; }
        DateTime? Since { get; set; }
        DateTime? Until { get; set; }
        string[] Arguments { get; set; }
        public QReview(QContext env, string text, Parsed[] args) : base(env, text, "review")
        {
            this.Arguments = new string[args.Length];
            for (int i = 0; i < args.Length; i++)
                this.Arguments[i] = args[i].text;
        }
        public override (bool ok, string message) Execute()
        {
            return (false, "Operation has not been implemented yet.");
        }
    }
}