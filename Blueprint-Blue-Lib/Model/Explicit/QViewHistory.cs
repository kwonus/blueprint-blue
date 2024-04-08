namespace Blueprint.Blue
{
    using Pinshot.PEG;
    public class QViewHistory : QView, ICommand     // QHistory object is the @view command for history
    {
        public UInt64 Id { get; protected set; }

        public QViewHistory(QContext env, string text, Parsed arg) : base(env, text)
        {
            this.Id = this.ParseId(arg);
        }
        private UInt64 ParseId(Parsed arg)
        {
            if (arg.rule == "id")
            {
                try
                {
                    return UInt64.Parse(arg.text);
                }
                catch
                {
                    ;
                }
            }
            return 0;
        }
        public override (bool ok, string message) Execute()
        {
            foreach (ExpandableHistory item in from entry in QContext.History.Values where entry.Id == this.Id select entry)
            {
                string html = item.AsHtml();
                return (true, html);
            }
            return (false, "Entry not found in history.");
        }
    }
}