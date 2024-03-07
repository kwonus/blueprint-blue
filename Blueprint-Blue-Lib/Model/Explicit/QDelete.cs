namespace Blueprint.Blue
{
    using Pinshot.PEG;

    public class QDeleteLabel : QSingleton, ICommand
    {
        public string Label { get; private set; } // macro

        public QDeleteLabel(QContext env, string text, Parsed[] args) : base(env, text, "delete")
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
    public class QDeleteHistory : QSingleton, ICommand
    {
        public UInt32 fromId { get; private set; }
        public UInt32 untilId { get; private set; }
        public DateTime? fromDate { get; private set; }
        public DateTime? untilDate { get; private set; }

        public QDeleteHistory(QContext env, string text, Parsed[] args) : base(env, text, "delete")
        {
            fromId = 0;
            untilId = 0;
            fromDate = null;
            untilDate = null;
        }
        public override (bool ok, string message) Execute()
        {
            return (false, "Operation has not been implemented yet.");
        }
    }
}