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
                int x = 0;
                int len = args[0].children.Length;
                if ((len >= 2) && (len <= 4)
                && args[0].rule.EndsWith("_set", StringComparison.InvariantCultureIgnoreCase))
                {
                    bool cmd = args[0].children[0].rule.EndsWith("_cmd");
                    if (!cmd)
                        x = 1;

                    string item = cmd ? args[0].children[0].rule.Replace("_cmd", "").Replace('_', '.') : x < len ? args[0].children[x].text.ToLower() : string.Empty;

                    this.Key = item;

                    Parsed? value = x+1 < len ? args[0].children[x+1] : null;

                    this.IsValid = (value != null) && !(string.IsNullOrWhiteSpace(value.text) || string.IsNullOrWhiteSpace(item));

                    this.Value = (value != null) && this.IsValid ? value.text.Trim() : string.Empty;
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