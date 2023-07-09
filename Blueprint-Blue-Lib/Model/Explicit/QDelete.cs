using Pinshot.PEG;
using XBlueprintBlue;

namespace Blueprint.Blue
{
    public class QDelete : QExplicitCommand, ICommand
    {
        public string Label { get; set; } // macro

        public QDelete(QContext env, string text, Parsed[] args) : base(env, text, "delete")
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                this.Label = string.Empty;
                return;
            }
            this.Label = args != null && args.Length == 1 ? args[0].text : string.Empty;
        }
        public override void AddArgs(XCommand command)
        {
            if (command != null && command.Arguments != null)
                command.Arguments.Add(this.Label);
        }
    }
}