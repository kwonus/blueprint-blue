namespace Blueprint.Blue
{
    using Pinshot.PEG;
    using XBlueprintBlue;

    public class QReset : QExplicitCommand, ICommand
    {
        public QReset(QContext env, string text, Parsed[] args) : base(env, text, "reset")
        {
            ;
        }
        public override void AddArgs(XCommand command)
        {
            ;
        }
    }
}