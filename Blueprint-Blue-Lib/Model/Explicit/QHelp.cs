using Pinshot.PEG;

namespace Blueprint.Blue
{
    public class QHelp : QExplicitCommand, ICommand
    {
        public string Topic { get; set; }
        public QHelp(QEnvironment env, string text, Parsed[] args) : base(env, text)
        {
            this.Verb = "help";
            this.Topic = args.Length == 1 ? args[0].text : "help";
            this.HelpDoc = "help.html";
        }
    }
}