using BlueprintBlue.Model.Implicit;
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
                if (args[0].children.Length == 3    // @set lexicon = av
                && args[0].children[0].rule.Equals("set_command", StringComparison.InvariantCultureIgnoreCase)
                && args[0].children[1].rule.EndsWith("_key", StringComparison.InvariantCultureIgnoreCase)
                && args[0].children[2].rule.EndsWith("_option", StringComparison.InvariantCultureIgnoreCase))
                {
                    this.IsValid = true;

                    if (string.IsNullOrWhiteSpace(args[0].children[1].text))
                    {
                        this.IsValid = false;
                        this.Key = "UNKNOWN";
                    }
                    else
                    {
                        this.Key = args[0].children[1].text;
                    }

                    if (string.IsNullOrWhiteSpace(args[0].children[2].text))
                    {
                        this.IsValid = false;
                        this.Value = string.Empty;
                    }
                    else
                    {
                        this.Value = args[0].children[2].text;
                    }
                    return;
                }

                else if (args[0].children.Length == 2    // @lexicon = av
                     && args[0].children[0].rule.EndsWith("_key", StringComparison.InvariantCultureIgnoreCase)
                     && args[0].children[1].rule.EndsWith("_option", StringComparison.InvariantCultureIgnoreCase))
                {
                    this.IsValid = true;

                    if (string.IsNullOrWhiteSpace(args[0].children[0].text))
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
                        this.Value = args[0].children[1].text;
                    }
                    return;
                }
            }
            this.IsValid = false;
            this.Key = string.Empty;
            this.Value = string.Empty;
        }
    }
}