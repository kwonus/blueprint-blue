using Pinshot.PEG;
using System.Collections.Generic;

namespace Blueprint.Blue
{
    public abstract class QImplicitCommand : QCommand, ICommand
    {
        public bool IsExplicit { get => false; }
        public abstract string Expand();

        public QImplicitCommand(QContext env, string text, string verb) : base(env, text, verb)
        {
            ;
        }
        public static IEnumerable<QImplicitCommand> Create(QContext env, Parsed clause)
        {
            var command = clause.rule.Trim().ToLower();

            QImplicitCommand? result = null;

            switch (command)
            {
                case "clear":       result = QVariable.Create(env, clause.text, clause.children);   break;
                case "invocation":  result = QInvoke.Create(env, clause.text, clause.children);     break; // invoke history/command or label/macro
                case "filter":      result = QFilter.Create(env, clause.text, clause.children);     break;
                case "search":      result = QFind.Create(env, clause.text, clause.children);       break;
                case "apply":       result = QApply.Create(env, clause.text, clause.children);      break; // apply macro
                case "setting":     result = QVariable.Create(env, clause.text, clause.children);   break;

                case "implicit_singletons":
                {
                    foreach (var subordinate in clause.children)
                    {
                        var sub = subordinate.rule.Trim().ToLower();

                        switch (sub)
                        {
                            case "print":  result = QLimit.Create(env, subordinate.text, subordinate.children);   break;
                            case "export": result = QExport.Create(env, subordinate.text, subordinate.children);    break;
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
            else
            {
                env.AddError("A coomand in the statement is ill-defined: " + clause.text);
            }
        }
    }
}