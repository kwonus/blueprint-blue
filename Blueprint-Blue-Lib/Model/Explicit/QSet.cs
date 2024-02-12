using Blueprint.Model.Implicit;
using Pinshot.PEG;
using YamlDotNet.Core.Tokens;

namespace Blueprint.Blue
{
    public class QSet : QExplicitCommand
    {
        public string Key { get; protected set; }
        public string Value { get; protected set; }
        public bool IsValid { get; protected set; }

        public QSet(QContext env, string text, Parsed[] args) : base(env, text, "set")
        {
            if (args.Length == 1)
            {
                if ((args[0].children.Length >= 2)    // @set lexicon = av
                && args[0].children[0].rule.EndsWith("_key", StringComparison.InvariantCultureIgnoreCase))
                {
                    this.IsValid = true;

                    if (string.IsNullOrWhiteSpace(args[0].children[1].text))
                    {
                        this.IsValid = false;
                        this.Key = "UNKNOWN";
                    }
                    else
                    {
                        this.Key = args[0].children[0].text;
                    }

                    if (string.IsNullOrWhiteSpace(args[0].children[1].text))
                    {
                        this.IsValid = false;
                        this.Value = string.Empty;
                    }
                    else
                    {
                        this.Value = args[0].children[1].rule;
                    }
                    return;
                }
            }
            this.IsValid = false;
            this.Key = string.Empty;
            this.Value = string.Empty;
        }
        public override (bool ok, string message) Execute()
        {
            bool ok = this.Context.GlobalSettings.Set(this);
            return (ok, ok ? string.Empty : "Could not save setting.");
        }
    }
}