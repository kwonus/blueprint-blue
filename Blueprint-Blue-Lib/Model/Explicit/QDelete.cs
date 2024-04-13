namespace Blueprint.Blue
{
    using AVSearch.Interfaces;
    using Pinshot.PEG;

    public class QDelete : QSingleton, ICommand
    {
        public UInt32? Since { get; protected set; }
        public UInt32? Until { get; protected set; }
        protected QDelete(QContext env, string text) : base(env, text, "delete")
        {
            this.Since = null;
            this.Until = null;
        }
        public static QDelete Create(QContext env, string text, Parsed[] args)
        {
            if (args.Length == 1)
            {
                switch (args[0].rule)
                {
                    case "macro_arg":   return new QDeleteMacro(env, text, args[0].children);
                    case "history_arg": return new QDeleteHistory(env, text, args[0].children);
                }
            }
            return new QDelete(env, text);  // this is a fail-safe error condition. The parse should NEVER lead us to here.
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
                        this.Since = (UInt32)((y * 100 * 100) + (m * 100) + d);
                    }
                }
                else if (this.Until == null && arg.rule == "date_until" && arg.children.Length == 1)
                {
                    if (GetYMD(arg.children[0].text, out y, out m, out d))
                    {
                        this.Until = (UInt32)((y * 100 * 100) + (m * 100) + d);
                    }
                }
            }
        }
        public override (bool ok, string message) Execute()
        {
            return (false, "Ambiguous target generated from parse.");
        }
    }
    public class QDeleteMacro : QDelete, ICommand
    {
        public string? Label { get; private set; } // macro
        public string? Wildcard { get; private set; } // macro

        public QDeleteMacro(QContext env, string text, Parsed[] args) : base(env, text)
        {
            this.Label = null;
            this.Wildcard = null;
            this.ParseDateRange(args);
            this.ParseLabel(args);
            if (this.Label == null)
                this.ParseWildcard(args);
        }
        protected void ParseWildcard(Parsed[] args)
        {
            foreach (Parsed arg in args)
            {
                if (arg.rule == "wildcard")
                {
                    this.Wildcard = arg.text;
                    break;
                }
            }
        }
        protected void ParseLabel(Parsed[] args)
        {
            foreach (Parsed arg in args)
            {
                if (arg.rule == "tag")
                {
                    this.Wildcard = arg.text;
                    break;
                }
            }
        }
        public override (bool ok, string message) Execute()
        {
            return (false, "Operation has not been implemented yet.");
        }
    }
    public class QDeleteHistory : QDelete, ICommand
    {
        public UInt64? From { get; private set; }
        public UInt64? Unto { get; private set; }
        public bool IsValid { get; private set; }

        public QDeleteHistory(QContext env, string text, Parsed[] args) : base(env, text)
        {
            this.From = 0;
            this.Unto = 0;
            this.IsValid = true;

            this.ParseDateRange(args);
            this.ParseIdRange(args);
        }
        protected void ParseIdRange(Parsed[] args)
        {
            UInt64 id;

            foreach (Parsed arg in args)
            {
                if (arg.rule == "id")
                {
                    try
                    {
                        id = UInt64.Parse(arg.text);
                        this.From = id;
                        this.Unto = id;
                        this.IsValid = true;    // even if we got an earlier exception deletion by explicit id should be fine
                        return;
                    }
                    catch
                    {
                        this.IsValid = false;
                    }
                }
                else if (this.From == null && arg.rule == "id_from" && arg.children.Length == 1)
                {
                    try
                    {
                        id = UInt64.Parse(arg.children[0].text);
                        this.From = id;
                    }
                    catch
                    {
                        this.IsValid = false;
                    }
                }
                else if (this.Unto == null && arg.rule == "id_until" && arg.children.Length == 1)
                {
                    try
                    {
                        id = UInt64.Parse(arg.children[0].text);
                        this.Unto = id;
                    }
                    catch
                    {
                        this.IsValid = false;
                    }
                }
            }
            return;
        }
        public override (bool ok, string message) Execute()
        {
            return (false, "Operation has not been implemented yet.");
        }
    }
}