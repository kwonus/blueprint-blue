using Pinshot.PEG;
using YamlDotNet.Serialization;

namespace Blueprint.Blue
{
    public class QApply : QImplicitCommand, ICommand
    {
        public string Label { get; set; }

        public override string Expand()
        {
            return this.Text;
        }
        private QApply(QContext env, string text, string label) : base(env, text, "apply")
        {
            this.Label = label;
        }
        public static QApply? Create(QContext env, string text, Parsed[] args)
        {
            if (args.Length == 1 && args[0].rule == "label")
            { 
                return new QApply(env, text, args[0].text);
            }
            return null;
        }
    }
}