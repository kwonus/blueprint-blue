namespace Blueprint.Blue
{
    using AVSearch.Interfaces;
    using Pinshot.PEG;
    public class QViewHistory : QView, ICommand     // QHistory object is the @view command for history
    {
        public QViewHistory(QContext env, string text, Parsed arg) : base(env, text)
        {
            this.Tag = this.ParseTag(arg);
        }
        private string ParseTag(Parsed arg)
        {
            if (arg.rule == "tag")
            {
                try
                {
                    return arg.text;
                }
                catch
                {
                    ;
                }
            }
            return string.Empty;
        }
        public override (bool ok, string message) Execute()
        {
            ExpandableHistory? item = ExpandableHistory.Deserialize(this.Tag);
            if (item != null)
            {
                string html = item.AsHtml();
                return (true, html);
            }
            return (false, "History tag not found.");
        }
    }
}