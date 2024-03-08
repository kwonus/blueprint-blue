namespace Blueprint.Model.Implicit
{
    using Blueprint.Blue;
    using Pinshot.PEG;

    public class QApply : QCommand
    {
        public string Label { get; internal set; }

        private QApply(QContext env, string text, string label) : base(env, text, "apply")
        {
            this.Label = label;
        }
        public static QApply? Create(QContext env, string text, Parsed arg)
        {
            if (arg.children.Length == 1 && arg.children[0].rule == "label")
            {
                string label = arg.children[0].text;
                return new QApply(env, text, label);
            }
            return null;
        }
    }
}