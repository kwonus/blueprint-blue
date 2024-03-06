namespace Blueprint.Blue
{
    using Pinshot.PEG;

    public abstract class QExplicitCommand : QCommand, ICommand
    {
        protected QExplicitCommand(QContext env, string text, string verb) : base(env, text, verb)
        {
            ;
        }
        public static QExplicitCommand? Create(QContext env, Parsed item)
        {
            if (item.rule.Equals("singleton", StringComparison.InvariantCultureIgnoreCase))
            {
                var commands = item.children;
                if (commands.Length == 1)
                {
                    var command = commands[0];
                    switch (command.rule.Trim().ToLower())
                    {
                        case "help":       return new QHelp(env,    command.text, command.children);
                        case "exit":       return new QExit(env,    command.text, command.children);
                        case "delete":     return new QDeleteLabel(env,  command.text, command.children); // TODO: handle DeleteHistory
                        case "review":     return new QReview(env,  command.text, command.children);
                        case "absorb":     return new QAbsorb(env,  command.text, command.children);
                        case "get":        return new QGet(env,     command.text, command.children);
                        case "set":        return new QSet(env,     command.text, command.children);
                        case "clear":      return new QClear(env,   command.text, command.children);
                        case "history":    return new QHistory(env, command.text, command.children);
                        case "invoke":     return new QHistory(env, command.text, command.children);
                    }
                }
            }
            return null;
        }

        public abstract (bool ok, string message) Execute();
    }
}