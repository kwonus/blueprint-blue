namespace Blueprint.Blue
{
    using AVSearch.Interfaces;
    using Pinshot.PEG;

    public class QViewMacro : QView, ICommand     // QReview object is the @view command for macros
    {
        public QViewMacro(QContext env, string text, Parsed arg) : base(env, text)
        {
            this.Tag = this.ParseLabel(arg);
        }
        private string ParseLabel(Parsed arg)
        {
            if (arg.rule == "tag")
            {
                return arg.text;
            }
            return string.Empty;
        }
        public override (bool ok, string message) Execute()
        {
            string label = this.Tag.Trim().ToLower();

            ExpandableMacro? macro = ExpandableMacro.Deserialize(label);
            if (macro != null)
            {
                string html = macro.AsHtml();
                return (true, html);
            }
            return (false, "Macro tag not found.");
        }
    }
}