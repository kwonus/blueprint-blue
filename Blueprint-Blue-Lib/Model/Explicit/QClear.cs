namespace Blueprint.Blue
{
    using AVSearch.Interfaces;
    using Pinshot.PEG;

    public class QClear : QSingleton, ICommand
    {
        private QContext Environment;
        public string Key { get; protected set; }
        public bool IsValid { get; protected set; }

        public QClear(QContext env, string text, Parsed[] args) : base(env, text, "clear")
        {
            this.Environment = env;

            if (args.Length == 2)
            {
                if ((args[0].rule == "clear_cmd") && args[1].rule.EndsWith("_key"))
                {
                    this.IsValid = true;

                    if (string.IsNullOrWhiteSpace(args[1].text))
                    {
                        this.IsValid = false;
                        this.Key = "UNKNOWN";
                    }
                    else
                    {
                        this.Key = args[1].text;
                    }
                    return;
                }
                else if ((args[0].rule == "clear_cmd") && args[1].rule.Equals("var"))
                {
                    this.IsValid = true;

                    if (string.IsNullOrWhiteSpace(args[1].text))
                    {
                        this.IsValid = false;
                        this.Key = "UNKNOWN";
                    }
                    else
                    {
                        this.Key = args[1].text;
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
            if ( (ok))
            {
                QGet get = new QGet(this.Environment, this.Text.Equals("all", StringComparison.InvariantCultureIgnoreCase) ? string.Empty : this.Text);
                return get.Execute();
            }
            return (ok, ok ? string.Empty : "Could not clear setting.");
        }
    }
}