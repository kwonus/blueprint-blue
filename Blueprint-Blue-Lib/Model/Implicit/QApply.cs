namespace Blueprint.Model.Implicit
{
    using Blueprint.Blue;
    using Pinshot.PEG;

    public class QApply : QCommand
    {
        public string Label { get; internal set; }
        public bool Full    {  get; internal set; }

        private QApply(QContext env, string text, string label, bool full) : base(env, text, "apply")
        {
            this.Label = label;
            this.Full = full;
        }
        public static QApply? Create(QContext env, string text, Parsed arg)
        {
            if (arg.children.Length == 1 && arg.children[0].children.Length == 1 && arg.children[0].children[0].rule == "label")
            {
                bool part = arg.rule.Equals("apply_macro_part", StringComparison.InvariantCultureIgnoreCase);
                bool full = arg.rule.Equals("apply_macro_full", StringComparison.InvariantCultureIgnoreCase);

                if (part || full)
                {
                    var label = arg.children[0].children[0];
                    return new QApply(env, text, label.text, full);
                }
            }
            return null;
        }
    }
}