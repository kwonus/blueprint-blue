namespace Blueprint.Blue
{
    public class QHelp : QExplicitCommand, ICommand
    {
        public string Topic { get; set; }
        public QHelp(QEnvironment env, string text, string topic) : base(env, text)
        {
            this.Topic = topic;
        }
    }
}