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
        public static QImplicitCommand? Create(QEnvironment env, Parsed command)
        {
            if (command.rule.Equals("implicit", StringComparison.InvariantCultureIgnoreCase))
            {
                switch (command.rule.Trim().ToLower())
                {
                    case "clear":   return QClear.Create(env,   command.text, command.children);
                    case "display": return QDisplay.Create(env, command.text, command.children);
                    case "exec":    return QExec.Create(env,    command.text, command.children);
                    case "filter":  return QFilter.Create(env,  command.text, command.children);
                    case "find":    return QFind.Create(env,    command.text, command.children);
                    case "invoke":  return QInvoke.Create(env,  command.text, command.children);
                    case "macro":   return QMacro.Create(env,   command.text, command.children);
                    case "set":     return QSet.Create(env,     command.text, command.children);
                }
            }
            return null;
        }
    }
}