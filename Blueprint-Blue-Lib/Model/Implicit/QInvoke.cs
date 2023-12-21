using Pinshot.PEG;
using YamlDotNet.Serialization;

namespace Blueprint.Blue
{
    public class QInvoke : QCommand, ICommand
    {
        public string Generic { get; private set; } // either
        public string? Label { get; private set; } // macro
        public UInt64? Id { get; private set; } // history
        public List<QFilter> Filters { get; private set; }
        public QSettings Settings { get; private set; }
        public QFind? Expression { get; private set; }

        private QInvoke(QContext env, string text, string invocation) : base(env, text, "invoke")
        {
            this.Id = null;
            this.Label = null;
            this.Generic = invocation.Trim();

            if (!string.IsNullOrEmpty(this.Generic))
            {
                if (this.Generic[0] >= '0' && this.Generic[0] <= '9')
                    this.Id = UInt64.Parse(this.Generic);
                else
                    this.Label = this.Generic;
            }
        }
        private QInvoke(QContext env, string text, uint id) : base(env, text, "invoke")
        {
            this.Id = id;
            this.Label = null;
            this.Generic = id.ToString();
        }

        public static QInvoke? Create(QContext env, string text, Parsed[] args, bool partial = true)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            string generic = args.Length >= 1 && args[0].rule.ToLower() == "invoke" && args[0].children.Length == 1 ? args[0].children[0].text : string.Empty;

            if (!string.IsNullOrWhiteSpace(generic))
            {
                bool numerics = args.Length >= 1 && args[0].children.Length == 1 && args[0].children[0].rule.ToLower() == "historic";
                bool labelled = args.Length >= 1 && args[0].children.Length == 1 && args[0].children[0].rule.ToLower() == "label";

                if (numerics || labelled)
                {
                    var invocation = new QInvoke(env, text, generic);

                    // TO DO:
                    // load settings from context
                    // load expression from context

                    return invocation;
                }
            }
            return null;
        }
    }
}