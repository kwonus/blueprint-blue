namespace Blueprint.Blue
{
    using Pinshot.PEG;

    public class QExit : QExplicitCommand, ICommand
    {
        public QExit(QContext env, string text, Parsed[] args) : base(env, text, "exit")
        {
            ;
        }
    }
}