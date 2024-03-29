namespace Blueprint.Blue
{
    using Blueprint.Blue;
    using Pinshot.PEG;
    using System;
    using YamlDotNet.Serialization;

    public enum FileCreateMode
    {
        CreateNew = 0,
        Overwrite = 1,
        Append = 2
    }
    public class QExport : QCommand
    {
        public string FileSpec { get; private set; }
        public FileCreateMode CreationMode { get; private set; }

        private QExport(QContext env, string text, string spec, FileCreateMode mode) : base(env, text, "export")
        {
            FileSpec = spec;
            CreationMode = mode;
        }
        public static QExport? Create(QContext env, string text, Parsed[] args)
        {
            if (args.Length == 1 && args[0].children.Length == 1 && args[0].children[0].rule.Equals("filename", StringComparison.InvariantCultureIgnoreCase))
            {
                var mode = FileCreateMode.CreateNew;

                if (args[0].rule.Equals("overwrite", StringComparison.InvariantCultureIgnoreCase))
                    mode = FileCreateMode.Overwrite;
                else if (args[0].rule.Equals("append", StringComparison.InvariantCultureIgnoreCase))
                    mode = FileCreateMode.Append;

                return new QExport(env, text, args[0].children[0].text, mode);
            }
            return null;
        }
        public override List<string> AsYaml()
        {
            return ICommand.YamlSerializer(this);
        }
    }

}