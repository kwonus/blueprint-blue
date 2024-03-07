namespace Blueprint.Blue
{
    using Pinshot.PEG;

    public class QClear : QSingleton, ICommand
    {
        public string Key { get; protected set; }
        public bool IsValid { get; protected set; }

        public QClear(QContext env, string text, Parsed[] args) : base(env, text, "clear")
        {
            if (args.Length == 1)
            {
                if (args[0].children.Length == 1
                && args[0].children[0].rule.EndsWith("_key", StringComparison.InvariantCultureIgnoreCase))
                {
                    this.IsValid = true;

                    if (string.IsNullOrWhiteSpace(args[0].children[0].text))
                    {
                        this.IsValid = false;
                        this.Key = "UNKNOWN";
                    }
                    else
                    {
                        this.Key = args[0].children[0].text;
                    }
                    return;
                }
            }
            this.IsValid = false;
            this.Key = string.Empty;
        }
        public override (bool ok, string message) Execute()
        {
            bool ok = this.Context.GlobalSettings.Clear(this);
            return (ok, ok ? string.Empty : "Could not clear setting.");
        }
    }
}