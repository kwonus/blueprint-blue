using Pinshot.PEG;
using XBlueprintBlue;

namespace Blueprint.Blue
{
    public class QAbsorb : QExplicitCommand, ICommand
    {
        public string Label { get; set; }
        public QAbsorb(QContext env, string text, Parsed[] args) : base(env, text, "absorb")
        {
            this.Label = args.Length == 1 ? args[0].text : "";
        }
        public override void AddArgs(XCommand command)
        {
            command.Arguments.Add(this.Label);
        }
    }
}