namespace Blueprint.Blue
{
    public class QFilter : QImplicitCommand, ICommand
    {
        public string Scope { get; set; }

        public QFilter(QEnvironment env, string text, string scope) : base(env, text)
        {
            this.Scope = scope;
        }
    }
}