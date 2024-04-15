namespace Blueprint.Blue
{
    using AVSearch.Interfaces;
    using Pinshot.PEG;
    using YamlDotNet.Core;

    public class QDelete : QSingleton, ICommand
    {
        public string Tag { get; protected set; } // macro

        protected QDelete(QContext env, string text) : base(env, text, "delete")
        {
            this.Tag = string.Empty;
        }
        public static QDelete Create(QContext env, string text, Parsed[] args)
        {
            if (args.Length == 2 && args[0].rule == "delete_cmd")
            {
                switch (args[1].rule)
                {
                    case "tag": return new QDeleteMacro(env, text, args[1]);
                    case "id":  return new QDeleteHistory(env, text, args[1]);
                }
            }
            return new QDelete(env, text);  // this is a fail-safe error condition. The parse should NEVER lead us to here.
        }
        public override (bool ok, string message) Execute()
        {
            return (false, "Ambiguous target generated from parse.");
        }
    }
    public class QDeleteMacro : QDelete, ICommand
    {
        public QDeleteMacro(QContext env, string text, Parsed arg) : base(env, text)
        {
            this.Tag = arg.text;
        }
        public override (bool ok, string message) Execute()
        {
            string yaml = Path.Combine(QContext.MacroPath, this.Tag + ".yaml");
            if (File.Exists(yaml))
            {
                try
                {
                    File.Delete(yaml);
                    return (true, "Macro has been deleted.");
                }
                catch
                {
                    return (false, "Unable to delete macro via the tag supplied. Do you have permission?");
                }
            }
            return (false, "Macro not found.");
        }
    }
    public class QDeleteHistory : QDelete, ICommand
    {
        public QDeleteHistory(QContext env, string text, Parsed arg) : base(env, text)
        {
            this.Tag = arg.text;
        }
        public override (bool ok, string message) Execute()
        {
            ExpandableHistory? item = ExpandableHistory.Deserialize(this.Tag);
            if (item != null)
            {
                string yaml = item.Id.AsYamlPath();
                try
                {
                    File.Delete(yaml);
                    return (true, "History item has been deleted.");
                }
                catch
                {
                    return (false, "Unable to delete history via the tag supplied. Do you have permission?");
                }
            }
            return (false, "History tag not found.");
        }
    }
}