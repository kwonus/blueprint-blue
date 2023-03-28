using Pinshot.PEG;

namespace Blueprint.Blue
{
    public class QExec : QImplicitCommand, ICommand
    {
        public uint Command { get; set; }

        private QExec(QContext env, string text, uint command) : base(env, text, "exec")
        {
            this.Command = command;
        }
        public static QExec Create(QContext env, string text, Parsed[] args)
        {
            return new QExec(env, text, 0);
        }
    }
}