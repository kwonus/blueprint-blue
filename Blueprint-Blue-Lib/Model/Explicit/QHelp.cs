namespace Blueprint.Blue
{
    using Pinshot.PEG;
    using System;

    public class QHelp : QSingleton, ICommand
    {
        public string Document { get; set; }
        public string Topic { get; set; }
        public QHelp(QContext env, string text, Parsed[] args) : base(env, text, "help")
        {
            if (args.Length == 1 && args[0].rule.Equals("topic"))
            {
                this.Topic = args[0].text.ToLower();
                this.Document = (args[0].children.Length == 1) && args[0].children[0].rule.StartsWith("doc_", StringComparison.InvariantCultureIgnoreCase) ? args[0].children[0].rule.Substring(4).ToLower() : string.Empty;
            }
            else
            {
                this.Topic = "";
                this.Document = "";
            }
        }
        public override (bool ok, string message) Execute()
        {
            return (true, "ok");
        }
    }
}