namespace Blueprint.Blue
{
    public class QExit : QExplicitCommand, ICommand
    {
        public QExit(QEnvironment env, string text) : base(env, text)
        {
            ;
        }
    }
}