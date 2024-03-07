namespace Blueprint.Blue
{
    using Pinshot.PEG;

    public class QGet : QSingleton, ICommand
    {
        public string Key { get; set; }
        public QGet(QContext env, string text, Parsed[] args) : base(env, text, "get")
        {
            this.Key = args.Length == 1  && args[0].children.Length == 1 && args[0].rule.Equals("var_get") ? args[0].children[0].text : "";
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