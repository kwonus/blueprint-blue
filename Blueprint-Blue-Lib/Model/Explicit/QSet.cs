using Blueprint.Model.Implicit;
using Pinshot.PEG;
using YamlDotNet.Core.Tokens;

namespace Blueprint.Blue
{
    public class QSet : QSingleton
    {
        public string Key { get; protected set; }
        public string Value { get; protected set; }
        public bool IsValid { get; protected set; }

        public QSet(QContext env, string text, Parsed[] args) : base(env, text, "set")
        {
            if (args.Length == 1)
            {
                int len = args[0].children.Length;
                if ((len >= 2) && (len <= 3)
                && args[0].rule.EndsWith("_set", StringComparison.InvariantCultureIgnoreCase))
                {
                    int strlen = args[0].rule.Length - "_set".Length;
                    string item = args[0].children[0].text;
                    if (len == 3)
                        item += ("." + args[0].children[1].text);
                    this.Key = item;

                    Parsed value = args[0].children[len - 1];

                    this.IsValid = !string.IsNullOrWhiteSpace(value.text);

                    this.Value = this.IsValid ? value.text.Trim() : string.Empty;
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