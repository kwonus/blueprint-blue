namespace Blueprint.Blue
{
    using AVSearch.Interfaces;
    using Pinshot.PEG;
    using System.Data;

    public class QControl : QSingleton, ICommand
    {
        public static string ExtractVerbFromRule(Parsed[] args)
        {
            if (args != null && args.Length == 1 && args[0].rule.EndsWith("_cmd") && args[0].rule.Length > "_cmd".Length)
                return args[0].rule.Substring(0, args[0].rule.Length - "_cmd".Length);
            return "unknown";
        }
        public QControl(QContext env, string text, Parsed[] args) : base(env, text, ExtractVerbFromRule(args))
        {
            ;
        }
        public override (bool ok, string message) Execute()
        {
            return (true, "ok");
        }
    }
}