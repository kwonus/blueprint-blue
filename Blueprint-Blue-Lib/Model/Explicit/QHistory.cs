namespace Blueprint.Blue
{
    using Pinshot.PEG;
    public class QHistory : QExplicitCommand, ICommand
    {
        public uint MaxCount      { get; private set; }
        public DateTime? Since    { get; private set; }
        public DateTime? Until    { get; private set; }
        public string[] Arguments { get; private set; }
        public bool reset         { get; private set; }
        public QHistory(QContext env, string text, Parsed[] args) : base(env, text, "history")
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