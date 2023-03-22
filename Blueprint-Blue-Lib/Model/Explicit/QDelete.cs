using Pinshot.PEG;

namespace Blueprint.Blue
{
    public class QDelete : QExplicitCommand, ICommand
    {
        public string Label { get; set; }
        public QDelete(QEnvironment env, string text, Parsed[] args) : base(env, text, "delete")
        {
            this.Label = args.Length == 1 ? args[0].text : "";
        }
    }
}