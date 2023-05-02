using Pinshot.PEG;
using XBlueprintBlue;

namespace Blueprint.Blue
{
    public class QExit : QExplicitCommand, ICommand
    {
        public QExit(QContext env, string text, Parsed[] args) : base(env, text, "exit")
        {
            ;
        }
        public override void AddArgs(XCommand command)
        {
           ;
        }
    }
}