namespace Blueprint.Blue
{
    public class QVersion : QExplicitCommand, ICommand
    {
        public bool Verbose { get; set; }
        public QVersion(QEnvironment env, string text, bool verbose) : base(env, text)
        {
            this.Verbose = verbose;
        }
    }
}