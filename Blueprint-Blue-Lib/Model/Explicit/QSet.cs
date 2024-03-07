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
                if ((args[0].children.Length >= 2) && (args[0].children.Length <= 3)
                &&   args[0].children[0].rule.EndsWith("_key", StringComparison.InvariantCultureIgnoreCase))
                {
                    if ((args[0].children.Length == 2)
                    &&   args[0].children[1].rule.Equals("similarity_opt", StringComparison.InvariantCultureIgnoreCase)
                    &&   args[0].children[1].children.Length == 1)
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
                            this.Value = args[0].children[1].text;
                        }
                    }
                    else if ((args[0].children.Length == 2)
                           && args[0].children[1].rule.StartsWith("similarity_", StringComparison.InvariantCultureIgnoreCase)
                           && args[0].children[1].children.Length >= 1)

                    {
                        string? word = null;
                        string? lemma = null;

                        var gchildren = args[0].children[1].children;
                        foreach (var gchild in gchildren)
                        {
                            if (gchild.children.Length == 2)
                            {
                                if (gchild.rule.EndsWith("_word", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    this.Key = args[0].children[0].text;
                                    word = gchild.children[1].text;
                                }
                                else if (gchild.rule.EndsWith("_lemma", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    this.Key = args[0].children[0].text;
                                    word = gchild.children[1].text;
                                }
                            }
                        }
                        this.IsValid = (word != null) || (lemma != null);

                        if (this.IsValid)
                        {
                            if ((word != null) && (lemma != null))
                                this.Value = "word:" + word + " lemma:" + lemma;
                            else if (word != null)
                                this.Value = "word:" + word;
                            else if (lemma != null)
                                this.Value = "lemma:" + lemma;
                        }
                        else
                        {
                            this.Key = "UNKNOWN";
                        }
                    }
                    else
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