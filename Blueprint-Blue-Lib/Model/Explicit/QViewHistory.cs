namespace Blueprint.Blue
{
    using Pinshot.PEG;
    public class QViewHistory : QView, ICommand     // QHistory object is the @view command for history
    {
        public UInt64 Id { get; protected set; }

        public QViewHistory(QContext env, string text, Parsed[] args) : base(env, text)
        {
            this.Id = 0;
            this.ParseId(args);
        }
        private void ParseId(Parsed[] args)
        {
            foreach (Parsed arg in args)
            {
                if (arg.rule == "id")
                {
                    try
                    {
                        this.Id = UInt64.Parse(arg.text);
                    }
                    catch
                    {
                        this.Id = 0;
                    }
                    break;
                }
            }
        }
        public override (bool ok, string message) Execute()
        {
            return (false, "Operation has not been implemented yet.");
        }
    }
}