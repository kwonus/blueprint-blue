namespace Blueprint.Blue
{
    using AVSearch.Interfaces;
    using Pinshot.PEG;
    public class QView : QSingleton, ICommand     // QHistory object is the @view command for history
    {
        public string Tag { get; protected set; }
        protected QView(QContext env, string text) : base(env, text, "view")
        {
            ;
        }
        public static QView Create(QContext env, string text, Parsed[] args)
        {
            if (args.Length == 2)
            {
                switch (args[1].rule)
                {
                    case "tag": return new QViewMacro(env, text, args[1]);
                    case "id":  return new QViewHistory(env, text, args[1]);
                }
            }
            return new QView(env, text);  // this is a fail-safe error condition. The parse should NEVER lead us to here.
        }
        public override (bool ok, string message) Execute()
        {
            return (false, "Ambiguous target generated from parse.");
        }
    }
}