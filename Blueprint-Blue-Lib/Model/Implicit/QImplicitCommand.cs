using Pinshot.PEG;

namespace Blueprint.Blue
{
    public class QImplicitCommand : QCommand, ICommand
    {
        public bool IsExplicit { get => false; }

        public QImplicitCommand(QContext env, string text, string verb) : base(env, text, verb)
        {
            ;
        }
        public static IEnumerable<QImplicitCommand> Create(QContext env, Parsed clause)
        {
            var command = clause.rule.Trim().ToLower();

            QImplicitCommand? result= null;

            switch (command)
            {
                case "clear":   result = QVariable.Create(env, clause.text, clause.children);      break;
                case "exec":    result = QExec.Create(env, clause.text, clause.children);       break;
                case "filter":  result = QFilter.Create(env, clause.text, clause.children);     break;
                case "search":  result = QFind.Create(env, clause.text, clause.children);       break;
                case "invoke":  result = QInvoke.Create(env, clause.text, clause.children);     break;
                case "macro":   result = QMacro.Create(env, clause.text, clause.children);      break;
                case "setting": result = QVariable.Create(env, clause.text, clause.children);        break;

                case "implicit_singletons":
                {
                    foreach (var subordinate in clause.children)
                    {
                        var sub = subordinate.rule.Trim().ToLower();

                        switch (sub)
                        {
                            case "print":  result = QDisplay.Create(env, subordinate.text, subordinate.children);   break;
                            case "export": result = QExport.Create(env, subordinate.text, subordinate.children); break;
                        }
                        if (result != null)
                        {
                            yield return result;
                            result = null;
                        }
                    }
                }   break;
            }
            if (result != null)
            {
                yield return result;
            }
        }
    }
}