namespace Blueprint.Model.Implicit
{
    using AVSearch.Interfaces;
    using Blueprint.Blue;
    using Pinshot.PEG;

    public class MacroDirective
    {
        public string Label { get; internal set; }

        private MacroDirective(QContext env, string text, string label)
        {
            this.Label = label;
        }
        public static MacroDirective? Create(QContext env, string text, Parsed arg)
        {
            if (arg.children.Length == 1 && arg.children[0].rule == "tag")
            {
                string label = arg.children[0].text;
                return new MacroDirective(env, text, label);
            }
            return null;
        }
    }
}