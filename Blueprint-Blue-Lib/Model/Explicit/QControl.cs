namespace Blueprint.Blue
{
    using AVSearch.Interfaces;
    using Pinshot.PEG;

    public class QControl : QSingleton, ICommand
    {
        public QControl(QContext env, string text, Parsed[] args) : base(env, text, "exit")
        {
            ;
        }
        public override (bool ok, string message) Execute()
        {
            return (true, "ok");
        }
    }
}