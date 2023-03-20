namespace Blueprint.Blue
{
    public class QReview : QExplicitCommand, ICommand
    {
        string[] Arguments { get; set; }
        public QReview(QEnvironment env, string text, string[] args) : base(env, text)
        {
            this.Arguments = args;
        }
    }
}