using Pinshot.PEG;
namespace Blueprint.Blue
{
    using System;
    using XBlueprintBlue;

    public abstract class QExplicitCommand : QCommand, ICommand
    {
        public bool IsExplicit { get => true; }
        public string Expand()
        {
            return this.Text;
        }
        protected QExplicitCommand(QContext env, string text, string verb) : base(env, text, verb)
        {
            ;
        }
        public XBlueprint AsSingletonCommand()
        {
            var reply = new XReply();
            var command = new XCommand() { Command = this.Text, Verb = this.Verb, Reply = reply };
            this.AddArgs(command);
            var request = this.Context.Statement.IsValid && (this.Context.Statement.Errors.Count == 0)
            ? new XBlueprint()
            {
                Settings = this.Context.AsMessage(),
                Singleton = command,
                Status = XStatusEnum.COMPLETED,
                Help = "https://to-be-defined-later.html"
            }
            : new XBlueprint()
            {
                Settings = this.Context.AsMessage(),
                Singleton = command,
                Status = XStatusEnum.ERROR,
                Errors = this.Context.Statement.Errors.Count > 0 ? this.Context.Statement.Errors : new() { "Unexpected error" },
                Help = "https://to-be-defined-later.html"
            };
            if (this.Context.Statement.Warnings.Count > 0)
            {
                request.Warnings = this.Context.Statement.Warnings;
            }
            return request;
        }
        public abstract void AddArgs(XCommand command);

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
                            case "version":    return new QVersion(env, command.text, command.children);
                            case "initialize": return new QInit(env,    command.text, command.children);
                            case "reset":      return new QReset(env,   command.text, command.children);
                        }
                    }
                }
            }
            return null;
        }
    }
}