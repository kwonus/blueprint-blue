using Pinshot.PEG;

namespace Blueprint.Blue
{
    public class QImplicitCommand : QCommand, ICommand
    {
        public bool IsExplicit { get => false; }

        public QImplicitCommand(QEnvironment env, string text, string verb) : base(env, text, verb)
        {
            ;
        }
        public static QImplicitCommand? Create(QEnvironment env, Parsed clause)
        {
            if (clause.rule.Equals("clause", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (var segment in clause.children)
                {
                    switch (segment.rule.Trim().ToLower())
                    {
                        case "clear":   return QClear.Create(env, clause.text, clause.children);
                        case "display": return QDisplay.Create(env, clause.text, clause.children);
                        case "exec":    return QExec.Create(env, clause.text, clause.children);
                        case "filter":  return QFilter.Create(env, clause.text, clause.children);
                        case "search":  return QFind.Create(env, clause.text, clause.children);
                        case "invoke":  return QInvoke.Create(env, clause.text, clause.children);
                        case "macro":   return QMacro.Create(env, clause.text, clause.children);
                        case "set":     return QSet.Create(env, clause.text, clause.children);
                    }
                }
            }
            return null;
        }
    }
}