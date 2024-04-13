namespace Blueprint.Blue
{
    using AVSearch.Interfaces;
    using Pinshot.PEG;

    public class QExit : QSingleton, ICommand
    {
        public QExit(QContext env, string text, Parsed[] args) : base(env, text, "exit")
        {
            ;
        }
        public override (bool ok, string message) Execute()
        {
            return (true, "ok");
        }
    }
}