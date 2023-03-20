namespace Blueprint.Blue
{
    public class QImplicitCommand : QCommand, ICommand
    {
        public bool IsExplicit { get => false; }

        public QImplicitCommand(QEnvironment env, string text) : base(env, text)
        {
            ;
        }
    }
}