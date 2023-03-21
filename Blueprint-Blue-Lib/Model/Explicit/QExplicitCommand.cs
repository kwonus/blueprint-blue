using Pinshot.PEG;

namespace Blueprint.Blue
{
    public class QExplicitCommand : QCommand, ICommand
    {
        public bool IsExplicit { get => true; }

        protected QExplicitCommand(QEnvironment env, string text) : base(env, text)
        {
            ;
        }
        public static QExplicitCommand? Create(QEnvironment env, Parsed item)
        {
            if (item.rule.Equals("statement", StringComparison.InvariantCultureIgnoreCase))
            {
                var statements = item.children;
                if ((statements.Length == 1) && statements[0].rule.Equals("explicit", StringComparison.InvariantCultureIgnoreCase))
                {
                    var explicits = statements[0].children;

                    if (explicits.Length == 1)
                    {
                        var command = explicits[0];
                        switch (command.rule.ToLower())
                        {
                            case "help": return new QHelp(env, command.text, command.children);
                            case "exit": return new QExit(env, command.text, command.children);
                        }
                    }
                }
            }
            switch(item.rule.ToLower())
            {
                case "help": return new QHelp(env, item.text, item.children);
                case "exit": return new QExit(env, item.text, item.children);
            }
            return null;
        }
    }
}