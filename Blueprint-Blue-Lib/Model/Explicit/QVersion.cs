namespace Blueprint.Blue
{
    using Pinshot.PEG;
    using XBlueprintBlue;

    public class QVersion : QExplicitCommand, ICommand
    {
        public bool Verbose { get; set; }
        public QVersion(QContext env, string text, Parsed[] args) : base(env, text, "review")
        {
            this.Verbose = false;
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i].text.Trim().ToLower().Replace("--", "-");
                if (arg == "-v" || arg == "-verbose")
                {
                    this.Verbose = true;
                    break;
                }
            }
        }
        public override void AddArgs(XCommand command)
        {
            if (this.Verbose)
                command.Arguments.Add("-verbose");
        }
    }
}