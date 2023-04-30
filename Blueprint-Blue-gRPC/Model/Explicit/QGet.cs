using Pinshot.PEG;

namespace Blueprint.Blue
{
    public class QGet : QExplicitCommand, ICommand
    {
        public string Key { get; set; }
        public QGet(QContext env, string text, Parsed[] args) : base(env, text, "get")
        {
            this.Key = args.Length == 1 ? args[0].text : "";
        }
    }
}