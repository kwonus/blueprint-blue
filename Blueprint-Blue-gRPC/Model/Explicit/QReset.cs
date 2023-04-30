namespace Blueprint.Blue
{
    using Pinshot.PEG;

    public class QReset : QExplicitCommand, ICommand
    {
        public QReset(QContext env, string text, Parsed[] args) : base(env, text, "reset")
        {
            ;
        }
    }
}