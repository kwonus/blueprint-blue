using Pinshot.PEG;

namespace Blueprint.Blue
{
    public class QInvoke : QImplicitCommand, ICommand
    {
        public uint Command { get; set; }

        private QInvoke(QContext env, string text, uint command) : base(env, text, "exec")
        {
            this.Command = command;
        }
        public static QInvoke Create(QContext env, string text, Parsed[] args)
        {
            return new QInvoke(env, text, 0);
        }
    }
}