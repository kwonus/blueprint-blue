namespace Blueprint.Blue
{
    public class QExec : QImplicitCommand, ICommand
    {
        public uint Command { get; set; }

        public QExec(QEnvironment env, string text, uint command) : base(env, text)
        {
            this.Command = command;
        }
    }
}