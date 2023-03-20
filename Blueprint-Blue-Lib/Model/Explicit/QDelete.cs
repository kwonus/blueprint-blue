namespace Blueprint.Blue
{
    public class QDelete : QExplicitCommand, ICommand
    {
        public string Label { get; set; }
        public QDelete(QEnvironment env, string text, string label) : base(env, text)
        {
            this.Label = label;
        }
    }
}