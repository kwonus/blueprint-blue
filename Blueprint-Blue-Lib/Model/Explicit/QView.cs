namespace Blueprint.Blue
{
    using Pinshot.PEG;
    public class QView : QSingleton, ICommand     // QHistory object is the @view command for history
    {
        public DateTime? Since    { get; protected set; }
        public DateTime? Until    { get; protected set; }
        protected QView(QContext env, string text) : base(env, text, "view")
        {
            this.Since = null;
            this.Until = null;
        }
        public static QView Create(QContext env, string text, Parsed[] args)
        {
            if (args.Length == 1)
            {
                switch (args[0].rule)
                {
                    case "macro_arg":   return new QViewMacro(env, text, args[0].children);
                    case "history_arg": return new QViewHistory(env, text, args[0].children);
                }
            }
            return new QView(env, text);  // this is a fail-safe error condition. The parse should NEVER lead us to here.
        }
        private static bool GetYMD(string input, out int y, out int m, out int d)
        {
            string[] dmy = input.Split('/');
            if (dmy.Length == 3)
            {
                try
                {
                    y = int.Parse(dmy[0]);
                    m = int.Parse(dmy[1]);
                    d = int.Parse(dmy[2]);
                    return true;
                }
                catch { }
            }
            y = 0;
            m = 0;
            d = 0;
            return false;
        }
        protected void ParseDateRange(Parsed[] args)
        {
            int y;
            int m;
            int d;

            foreach (Parsed arg in args)
            {
                if (this.Since == null && arg.rule == "date_from" && arg.children.Length == 1)
                {
                    if (GetYMD(arg.children[0].text, out y, out m, out d))
                    {
                        this.Since = new DateTime(y, m, d);
                    }
                }
                else if (this.Until == null && arg.rule == "date_until" && arg.children.Length == 1)
                {
                    if (GetYMD(arg.children[0].text, out y, out m, out d))
                    {
                        this.Until = new DateTime(y, m, d);
                    }
                }
            }
        }
        public override (bool ok, string message) Execute()
        {
            return (false, "Ambiguous target generated from parse.");
        }
    }
}