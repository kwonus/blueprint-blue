namespace Blueprint.Blue
{
    using AVSearch.Interfaces;
    using BlueprintBlue.Model;
    using Pinshot.PEG;
    using System;

    public class QBulkHistory : QBulk, ICommand     // QHistory object is the @view command for history
    {
        public QBulkHistory(QContext env, string text, Parsed[] args, BulkAction action) : base(env, text, "history", action)
        {
            this.ParseDateRange(args);
        }
        public override (bool ok, string message) Execute()
        {
            IEnumerable<ExpandableInvocation> history;

            history = QContext.GetHistory(this.NumericFrom, this.NumericUnto);

            string html = history != null ? ExpandableInvocation.AsBulkHtml(history, typeof(ExpandableHistory)) : string.Empty;

            return (true, html);
        }
    }
}