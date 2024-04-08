namespace Blueprint.Blue
{
    using Pinshot.PEG;

    public class QViewMacro : QView, ICommand     // QReview object is the @view command for macros
    {
        public string Label { get; private set; } // macro

        public QViewMacro(QContext env, string text, Parsed arg) : base(env, text)
        {
            this.Label = this.ParseLabel(arg);
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
            string label = this.Label.Trim().ToLower();
            if (QContext.Macros.ContainsKey(label))
            {
                string html = QContext.Macros[label].AsHtml();
                return (true, html);
            }
            return (false, "Entry not found in history.");
        }
    }
}