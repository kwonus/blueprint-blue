namespace Blueprint.Blue
{
    using AVSearch.Interfaces;
    using Pinshot.PEG;

    public abstract class QSingleton : QCommand, ICommand
    {
        protected QSingleton(QContext env, string text, string verb) : base(env, text, verb)
        {
            ;
        }
        public static QSingleton? Create(QContext env, Parsed item)
        {
            if (item.rule.Equals("singleton", StringComparison.InvariantCultureIgnoreCase))
            {
                var commands = item.children;
                if (commands.Length == 1)
                {
                    var command = commands[0];
                    switch (command.rule.Trim().ToLower())
                    {
                        case "help":       return new QHelp(env,      command.text, command.children);
                        case "exit":       return new QExit(env,      command.text, command.children);
                        case "absorb":     return new QAbsorb(env,    command.text, command.children);
                        case "get":        return new QGet(env,       command.text, command.children);
                        case "set":        return new QSet(env,       command.text, command.children);
                        case "clear":      return new QClear(env,     command.text, command.children);

                        case "delete_bulk":return QBulk.Create(env, command.text, command);
                        case "delete":     return QDelete.Create(env, command.text, command.children);

                        case "view_bulk":  return QBulk.Create(env, command.text, command);
                        case "view":       return QView.Create(env, command.text, command.children);
                    }
                }
            }
            return null;
        }

        public abstract (bool ok, string message) Execute();
    }
}