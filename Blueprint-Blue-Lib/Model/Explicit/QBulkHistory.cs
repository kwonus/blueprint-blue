namespace Blueprint.Blue
{
    using AVSearch.Interfaces;
    using BlueprintBlue.Model;
    using Pinshot.PEG;
    using System;

    public class QBulkHistory : QBulk, ICommand     // QHistory object is the @view command for history
    {
        public QID? From { get; protected set; }
        public QID? Unto { get; protected set; }
        public QBulkHistory(QContext env, string text, Parsed[] args, BulkAction action) : base(env, text, "history", action)
        {
            this.From = null;
            this.Unto = null;

            this.ParseDateRange(args);
            this.ParseIdRange(args);
        }
        protected void ParseIdRange(Parsed[] args)
        {
            foreach (Parsed arg in args)
            {
                if (this.From == null && arg.rule == "id_from" && arg.children.Length == 1)
                {
                    try
                    {
                        this.From = new QID(arg.children[0].text);
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
                        this.Unto = new QID(arg.children[0].text);
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
            IEnumerable<ExpandableInvocation> history;

            if (this.From != null || this.Unto != null)
            {
                Dictionary<string, QID> range = new();
                if (this.From != null)
                    range["from"] = this.From;
                if (this.Unto != null)
                    range["to"] = this.Unto;

                history = QContext.GetHistory(range);
            }
            else
            {
                history = QContext.GetHistory(this.NumericFrom, this.NumericUnto);
            }

            string html = history != null ? ExpandableInvocation.AsBulkHtml(history, typeof(ExpandableHistory)) : string.Empty;

            return (true, html);
        }
    }
}