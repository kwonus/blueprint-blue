namespace Blueprint.Blue
{
    using Pinshot.PEG;
    public class QView : QSingleton, ICommand     // QHistory object is the @view command for history
    {
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
                    case "tag": return new QViewMacro(env, text, args[0].children);
                    case "id":  return new QViewHistory(env, text, args[0].children);
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