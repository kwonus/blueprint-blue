namespace Blueprint.Blue
{
    using Pinshot.PEG;
    public class QViewHistory : QView, ICommand     // QHistory object is the @view command for history
    {
        public UInt64? From { get; protected set; }
        public UInt64? Unto { get; protected set; }
        public QViewHistory(QContext env, string text, Parsed[] args) : base(env, text)
        {
            this.From = null;
            this.Unto = null;

            this.ParseDateRange(args);
            this.ParseIdRange(args);
        }
        protected void ParseIdRange(Parsed[] args)
        {
            UInt64 id;

            foreach (Parsed arg in args)
            {
                if (arg.rule == "id")
                {
                    try
                    {
                        id = UInt64.Parse(arg.text);
                        this.From = id;
                        this.Unto = id;
                        return;
                    }
                    catch
                    {
                        ;
                    }
                }
                else if (this.From == null && arg.rule == "id_from" && arg.children.Length == 1)
                {
                    try
                    {
                        id = UInt64.Parse(arg.children[0].text);
                        this.From = id;
                    }
                    catch
                    {
                        ;
                    }
                }
                else if (this.Unto == null && arg.rule == "id_until" && arg.children.Length == 1)
                {
                    try
                    {
                        id = UInt64.Parse(arg.children[0].text);
                        this.Unto = id;
                    }
                    catch
                    {
                        ;
                    }
                }
            }
            return;
        }
        public override (bool ok, string message) Execute()
        {
            return (false, "Operation has not been implemented yet.");
        }
    }
}