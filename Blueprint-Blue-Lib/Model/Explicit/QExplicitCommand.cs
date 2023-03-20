namespace Blueprint.Blue
{
    public class QExplicitCommand : QCommand, ICommand
    {
        public bool IsExplicit { get => true; }

        public QExplicitCommand(QEnvironment env, string text) : base(env, text)
        {
            ;
        }
    }
}