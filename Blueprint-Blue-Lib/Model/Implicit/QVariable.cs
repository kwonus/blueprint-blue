using Pinshot.PEG;

namespace Blueprint.Blue
{
    public class QVariable : QImplicitCommand, ICommand
    {
        public string Key { get; protected set; }
        public string Value { get; protected set; }
        public bool Persistent { get; protected set; }

        protected QVariable(QEnvironment env, string text, string verb, string key, string value, bool persistent) : base(env, text, verb)
        {
            this.Key = key;
            this.Value = value;
            this.Persistent = persistent;
        }
        public static QVariable? Create(QEnvironment env, string text, Parsed[] args)
        {
            if((args.Length == 1)
            && (args[0].children.Length == 2)
            &&  args[0].children[0].rule.EndsWith("_key", StringComparison.InvariantCultureIgnoreCase)
            &&  args[0].children[1].rule.EndsWith("_option", StringComparison.InvariantCultureIgnoreCase))
            {
                switch (args[0].children[1].text.ToLower())
                {
                    case "default":
                        if (args[0].rule.EndsWith("_set"))
                            return new QClear(env, text, args[0].children[0].text, true);
                        else if (args[0].rule.EndsWith("_var"))
                            return new QClear(env, text, args[0].children[0].text, true);
                        break;

                    default:
                        if (args[0].rule.EndsWith("_set"))
                            return new QSet(env, text, args[0].children[0].text, args[0].children[1].text, true);
                        else if (args[0].rule.EndsWith("_var"))
                            return new QSet(env, text, args[0].children[0].text, args[0].children[1].text, false);
                        break;
                }
            }
            return null;
        }
    }
}