namespace BlueprintBlue.Model.Results
{
    using Blueprint.Blue;
    using Blueprint.Model.Implicit;
    using System.Text;
    using Pinshot.Blue;

    public enum QCommandCategory
    {
        SEARCH = 0,
        SYSTEM = 1,
        CONTROLS = 2,
        LABELING = 3,
        HISTORY = 4,
        OUTPUT = 5,
    }
    public enum QCommandPoutFileMode
    {
        CREATE = 0,
        OVERWRITE = 1,
        APPEND = 2,
    }
    public abstract class QExplicitResult
    {
        public QCommandCategory Category { get; private set; }
        public string Verb;
        protected QStatement Statement { get; private set; }
        protected QSingleton Command { get; private set; }
        public abstract string GetResponse();

        public bool IsValid { get => this.Statement.IsValid; }
        public string ParseDiagnostic { get => this.Statement.ParseDiagnostic; }
        public List<string> Errors { get => this.Statement.Errors; }
        public List<string> Warnings { get => this.Statement.Warnings; }
        public Dictionary<string, string> Disposition { get => this.Statement.Disposition; }
        protected QExplicitResult(QSingleton command, QStatement stmt, string path, QCommandCategory category)
        {
            this.Category = category;
            this.Verb = command.Verb;
            this.Statement = stmt;
            this.Command = command;
        }
        protected static QExplicitResult Create(QHelp help, QStatement stmt)
        {
            string path = string.Empty;
            return new QSystemResult(help, stmt, path);
        }
        protected static QExplicitResult Create(QExit exit, QStatement stmt)
        {
            string path = string.Empty;
            return new QSystemResult(exit, stmt, path);
        }
        protected static QExplicitResult Create(QDeleteLabel delete, QStatement stmt)
        {
            string path = string.Empty;
            return new QLabelResult(delete, stmt, path);
        }
        protected static QExplicitResult Create(QDeleteHistory delete, QStatement stmt)
        {
            string path = string.Empty;
            return new QHistoryResult(delete, stmt, path);
        }
        protected static QExplicitResult Create(QReview review, QStatement stmt)
        {
            string path = string.Empty;
            return new QLabelResult(review, stmt, path);
        }
        protected static QExplicitResult Create(QAbsorb absorb, QStatement stmt)
        {
            string path = string.Empty;
            return new QControlsResult(absorb, stmt, path);
        }
        protected static QExplicitResult Create(QGet qget, QStatement stmt)
        {
            string path = string.Empty;
            return new QControlsResult(qget, stmt, path);
        }
        protected static QExplicitResult Create(QSet qset, QStatement stmt)
        {
            string path = string.Empty;
            return new QControlsResult(qset, stmt, path);
        }
        protected static QExplicitResult Create(QClear clear, QStatement stmt)
        {
            string path = string.Empty;
            return new QControlsResult(clear, stmt, path);
        }
        protected static QExplicitResult Create(QHistory history, QStatement stmt)
        {
            return new QHistoryResult(history, stmt, string.Empty);
        }
        public static QExplicitResult? Create(QSingleton command, QStatement stmt)
        {
            if (command.GetType() == typeof(QHelp))
                return Create((QHelp)command, stmt);
            if (command.GetType() == typeof(QExit))
                return Create((QExit)command, stmt);
            if (command.GetType() == typeof(QDeleteLabel))
                return Create((QDeleteLabel)command, stmt);
            if (command.GetType() == typeof(QReview))
                return Create((QReview)command, stmt);
            if (command.GetType() == typeof(QGet))
                return Create((QGet)command, stmt);
            if (command.GetType() == typeof(QSet))
                return Create((QSet)command, stmt);
            if (command.GetType() == typeof(QClear))
                return Create((QClear)command, stmt);
            if (command.GetType() == typeof(QHistory))
                return Create((QHistory)command, stmt);
 //         if (command.GetType() == typeof(QHistoryResult))
 //             return Create((QHistoryResult)command, stmt);

            return null;
        }
    }
    public class QLabelResult : QExplicitResult
    {
        string Label;

        public QLabelResult(QSingleton command, QStatement stmt, string path) : base(command, stmt, path, QCommandCategory.LABELING)
        {
            if (command.GetType() == typeof(QDeleteLabel))
                this.Label = ((QDeleteLabel)command).Label;
            else if (command.GetType() == typeof(QReview))
                this.Label = ((QReview)command).Label;
            else
                this.Label = string.Empty;
        }
        public override string GetResponse()
        {
            if (this.IsValid)
            {
                if (this.Command.GetType() == typeof(QReview))
                {
                    QReview review = (QReview) this.Command;
                    ExpandableInvocation macro = this.Command.Context.GetMacro(this.Label);

                    YamlDotNet.Serialization.Serializer serializer = new();
                    string yaml = serializer.Serialize(macro);
                    return yaml;
                }
                else return "ok";
            }
            return "error";
        }
    }
    public class QHistoryResult : QExplicitResult
    {
        public QHistoryResult(QSingleton command, QStatement stmt, string path) : base(command, stmt, path, QCommandCategory.HISTORY)
        {
            ;
        }
        public override string GetResponse()
        {
            if (this.Command.GetType() == typeof(QHistory))
            {
                QHistory history = (QHistory)this.Command;
                StringBuilder result = new StringBuilder(512);

                bool filterOnId = history.From != null || history.Unto != null;
                bool filterOnDate = history.From != null || history.Unto != null;
            }
            return this.IsValid ? "ok" : "error";
        }
    }
    public class QControlsResult : QExplicitResult
    {
        string Setting;
        public QControlsResult(QSingleton command, QStatement stmt, string path) : base(command, stmt, path, QCommandCategory.CONTROLS)
        {
            if (command.GetType() == typeof(QSet))
                this.Setting = ((QSet)command).Key;
            else if (command.GetType() == typeof(QGet))
                this.Setting = ((QGet)command).Key;
            else if (command.GetType() == typeof(QClear))
                this.Setting = ((QClear)command).Key;
            else if (command.GetType() == typeof(QAbsorb))
                this.Setting = "*";
            else
                this.Setting = string.Empty;
        }
        public override string GetResponse()
        {
            if (this.IsValid)
            {
                UInt32 revision = Pinshot_RustFFI.get_library_revision();
                string version = revision.ToString("X4");
                string[] keys = new string[] { QSpan.Name, QLexicalDomain.Name, QLexicalDisplay.Name, QFormat.Name, QSimilarity.Name, "version" };
                string[] vals = new string[6];
                for (int i = 0; i < vals.Length; i++)
                {
                    switch (i)
                    {
                        case 0: vals[i] = this.Command.Context.GlobalSettings.Span.ToString(); break;
                        case 1: vals[i] = this.Command.Context.GlobalSettings.Lexicon.ToString(); break;
                        case 2: vals[i] = this.Command.Context.GlobalSettings.Display.ToString(); break;
                        case 3: vals[i] = this.Command.Context.GlobalSettings.Format.ToString(); break;
                        case 4: vals[i] = this.Command.Context.GlobalSettings.Similarity.ToString(); break;
                        case 5: vals[i] = version[0] + "." + version[1] + "." + version.Substring(2); break;
                    }
                }
                for (int i = 0; i < vals.Length; i++)
                {
                    if (vals[i].Equals(this.Setting))
                    {
                        vals[i] = "**" + vals[i] + "**";
                        break;
                    }
                }
                string table = this.Command.Context.GlobalSettings.AsMarkdown(showDefaults: true, showExtendedSettings: true, bold: this.Setting);
                return table;
            }
            else
            {
                return "error";
            }
        }
    }
    public class QSystemResult : QExplicitResult
    {
        string Parameter;
        public QSystemResult(QSingleton command, QStatement stmt, string path) : base(command, stmt, path, QCommandCategory.SYSTEM)
        {
            if (command.GetType() == typeof(QHelp))
                this.Parameter = ((QHelp)command).Topic;
            else
                this.Parameter = string.Empty;
        }
        public override string GetResponse()
        {
            if (this.Command.GetType() == typeof(QHistory))
            {
                QHistory history = (QHistory)this.Command;
                StringBuilder result = new StringBuilder(512);

                bool filterOnId = history.From != null || history.Unto != null;
                bool filterOnDate = history.From != null || history.Unto != null;
            }
            return this.IsValid ? "ok" : "error";
        }
    }
}
