using Pinshot.PEG;

namespace Blueprint.Blue
{
    public class QExit : QExplicitCommand, ICommand
    {
        public QExit(QEnvironment env, string text, Parsed[] args) : base(env, text, "exit")
        {
            ;
        }
    }
}