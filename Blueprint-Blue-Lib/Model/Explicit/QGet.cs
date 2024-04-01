namespace Blueprint.Blue
{
    using Pinshot.PEG;

    public class QGet : QSingleton, ICommand
    {
        public string Key { get; set; }
        public QGet(QContext env, string text, Parsed[] args) : base(env, text, "get")
        {
            switch (args[0].children.Length)
            {
                case 2:
                    if (args[0].rule.EndsWith("_get"))
                    {
                        if (args[0].children[1].rule.EndsWith("_key"))
                        {
                            this.Key = args[0].children[1].text.ToLower();
                            return;
                        }
                    } break;
                case 1:
                    if (args[0].children[0].rule.EndsWith("_key"))
                    {
                        this.Key = args[0].children[0].text.ToLower();
                        return;
                    } break;
            }
            this.Key = string.Empty;
        }
        public override (bool ok, string message) Execute()
        {
            string result = this.Context.GlobalSettings.Get(this);
            if (!string.IsNullOrEmpty(result))
            {
                Console.WriteLine(result);
            }
            return (!string.IsNullOrEmpty(result), result);
        }
    }
}