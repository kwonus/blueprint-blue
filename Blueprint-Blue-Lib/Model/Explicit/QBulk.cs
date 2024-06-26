namespace Blueprint.Blue
{
    using AVSearch.Interfaces;
    using Pinshot.PEG;
    public enum BulkAction
    {
        Undefined = 0,
        View = 1,
        Delete = -1,
    }
    public class QBulk : QSingleton, ICommand     // QHistory object is the @view command for history
    {
        public UInt32? NumericFrom { get; protected set; }
        public UInt32? NumericUnto { get; protected set; }
        public BulkAction Action   { get; protected set; }
        protected QBulk(QContext env, string text, string cmd, BulkAction action) : base(env, text, cmd)
        {
            this.NumericFrom = null;
            this.NumericUnto = null;
            this.Action = action;
        }
        public static QBulk Create(QContext env, string text, Parsed arg)
        {
            BulkAction action = BulkAction.View;
            foreach (Parsed parse in arg.children)
            {
                if (parse.rule == "delete_arg")
                {
                    action = BulkAction.Delete;
                    break;
                }
            }
            foreach (Parsed parse in arg.children)
            {
                switch (parse.rule)
                {
                    case "macros_cmd":  return new QBulkMacros(env, text, arg.children, action);
                    case "history_cmd": return new QBulkHistory(env, text, arg.children, action);
                }
            }
            return new QBulk(env, text, string.Empty, BulkAction.Undefined);  // this is a fail-safe error condition. The parse should NEVER lead us to here.
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
                if (this.NumericFrom == null && arg.rule == "date_from" && arg.children.Length == 1)
                {
                    if (GetYMD(arg.children[0].text, out y, out m, out d))
                    {
                        this.NumericFrom = (UInt32)((y * 100 * 100) + (m * 100) + d);
                    }
                }
                else if (this.NumericUnto == null && arg.rule == "date_until" && arg.children.Length == 1)
                {
                    if (GetYMD(arg.children[0].text, out y, out m, out d))
                    {
                        this.NumericUnto = (UInt32)((y * 100 * 100) + (m * 100) + d);
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