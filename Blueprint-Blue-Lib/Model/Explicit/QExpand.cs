using Pinshot.PEG;

namespace Blueprint.Blue
{
    public class QExpand : QExplicitCommand, ICommand
    {
        public string Label { get; set; }
        public QExpand(QEnvironment env, string text, Parsed[] args) : base(env, text, "expand")
        {
            this.Label = args.Length == 1 ? args[0].text : "";
        }
    }
}