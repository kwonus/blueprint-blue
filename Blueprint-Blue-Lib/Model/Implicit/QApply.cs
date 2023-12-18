using Pinshot.PEG;
using YamlDotNet.Serialization;

namespace Blueprint.Blue
{
    public class QApply : QCommand
    {
        public string Label { get; internal set; }
        public bool Full    {  get; internal set; }

        private QApply(QContext env, string text, string label, bool full) : base(env, text, "apply")
        {
            this.Label = label;
            this.Full = full;
        }
        public static QApply? Create(QContext env, string text, Parsed[] args)
        {
            if (args.Length == 2 && args[1].rule == "apply_macro_full" && args[1].children.Length == 1 && args[1].children[0].rule == "label_full")
            { 
                return new QApply(env, text, args[1].children[0].text, true);
            }
            else if (args.Length == 2 && args[1].rule == "apply_macro_part" && args[1].children.Length == 1 && args[1].children[0].rule == "label_part")
            {
                return new QApply(env, text, args[1].children[0].text, false);
            }
            return null;
        }
    }
}