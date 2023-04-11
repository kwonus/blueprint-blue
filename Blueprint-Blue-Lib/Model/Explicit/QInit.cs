namespace Blueprint.Blue
{
    using Pinshot.PEG;

    public class QInit : QExplicitCommand, ICommand
    {
        public string[] Arguments { get; set; }
        public bool IsValid
        {
            get => (this.Arguments.Length == 1) && this.Arguments[0].Equals("history", StringComparison.InvariantCultureIgnoreCase);
        }
        public QInit(QContext env, string text, Parsed[] args) : base(env, text, "initialize")
        {
            this.Arguments = new string[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                this.Arguments[i] = args[i].text.Trim().ToLower();
            }
        }
    }
}