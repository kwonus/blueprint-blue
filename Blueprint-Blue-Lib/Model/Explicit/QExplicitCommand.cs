namespace Blueprint.Blue
{
    using Pinshot.PEG;

    public abstract class QExplicitCommand : QCommand, ICommand
    {
        public string Expand()
        {
            return this.Text;
        }
        protected QExplicitCommand(QContext env, string text, string verb) : base(env, text, verb)
        {
            ;
        }
        public static QExplicitCommand? Create(QContext env, Parsed item)
        {
            if (item.rule.Equals("statement", StringComparison.InvariantCultureIgnoreCase))
            {
                var commands = item.children;
                if ((commands.Length == 1) && commands[0].rule.Equals("singleton", StringComparison.InvariantCultureIgnoreCase))
                {
                    var explicits = commands[0].children;

                    if (explicits.Length == 1)
                    {
                        var command = explicits[0];
                        switch (command.rule.Trim().ToLower())
                        {
                            case "help":       return new QHelp(env,    command.text, command.children);
                            case "exit":       return new QExit(env,    command.text, command.children);
                            case "delete":     return new QDelete(env,  command.text, command.children);
                            case "expand":     return new QExpand(env,  command.text, command.children);
                            case "absorb":     return new QAbsorb(env,  command.text, command.children);
                            case "get":        return new QGet(env,     command.text, command.children);
                            case "review":     return new QReview(env,  command.text, command.children);
                            case "set":        return new QSet(env,     command.text, command.children); // changed for revision #3C15 (was version)
                            case "initialize": return new QInit(env,    command.text, command.children);
                            case "clear":      return new QClear(env,   command.text, command.children); // changed for revision #3C15 (was reset)
                        }
                    }
                }
            }
            return null;
        }
    }
}