namespace Blueprint.Blue
{
    public class QInvoke : QImplicitCommand, ICommand
    {
        public string Label { get; set; }

        public QInvoke(QEnvironment env, string text, string label) : base(env, text)
        {
            this.Label = label;
        }
    }
}