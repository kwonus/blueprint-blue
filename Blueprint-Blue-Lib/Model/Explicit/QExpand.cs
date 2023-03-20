namespace Blueprint.Blue
{
    public class QExpand : QExplicitCommand, ICommand
    {
        public string Label { get; set; }
        public QExpand(QEnvironment env, string text, string label) : base(env, text)
        {
            this.Label = label;
        }
    }
}