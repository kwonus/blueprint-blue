namespace Blueprint.Blue
{
    public class QExport : QImplicitCommand, ICommand
    {
        public string FileSpec { get; set; }

        public QExport(QEnvironment env, string text, string spec) : base(env, text)
        {
            this.FileSpec = spec;
        }
    }
}