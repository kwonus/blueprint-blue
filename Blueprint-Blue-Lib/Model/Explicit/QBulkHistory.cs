namespace Blueprint.Blue
{
    using Pinshot.PEG;
    public class QBulkHistory : QBulk, ICommand     // QHistory object is the @view command for history
    {
        public UInt64? From { get; protected set; }
        public UInt64? Unto { get; protected set; }
        public QBulkHistory(QContext env, string text, Parsed[] args, BulkAction action) : base(env, text, "history", action)
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
                if (this.From == null && arg.rule == "id_from" && arg.children.Length == 1)
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
            IEnumerable<ExpandableInvocation> history
                = (this.From != null && this.Unto != null) ? QContext.GetHistory(idFrom: this.From.Value, idUnto: this.Unto.Value)
                : (this.From != null)                      ? QContext.GetHistory(idFrom: this.From.Value)
                : (this.Unto != null)                      ? QContext.GetHistory(idUnto: this.Unto.Value)
                : QContext.GetHistory(notBefore: this.Since, notAfter: this.Until);

            string html = ExpandableHistory.AsBulkHtml(history, "id");

            return (true, html);
        }
    }
}