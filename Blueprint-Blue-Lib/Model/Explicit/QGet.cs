namespace Blueprint.Blue
{
    using AVSearch.Interfaces;
    using Pinshot.PEG;
    using static System.Net.Mime.MediaTypeNames;

    public class QGet : QSingleton, ICommand
    {
        public string Key { get; set; }
        internal QGet(QContext env, string text) : base(env, text, "get")
        {
            this.Key = string.Empty;
        }

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
            string result = this.Context.GlobalSettings.Get(this, Model.Implicit.QFormat.QFormatVal.MD);
            if (!string.IsNullOrEmpty(result))
            {
                Console.WriteLine(result);
            }
            return (!string.IsNullOrEmpty(result), result);
        }
    }
}