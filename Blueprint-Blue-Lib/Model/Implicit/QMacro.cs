namespace Blueprint.Blue
{
    public class QMacro : QImplicitCommand, ICommand
    {
        public string Label { get; set; }

        public QMacro(QEnvironment env, string text, string label) : base(env, text)
        {
            this.Label = label;
        }
    }
}