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
        public string FilePath { get; protected set; }
        protected QStatement Statement { get; private set; }
        protected QExplicitCommand Command { get; private set; }
        public abstract string GetResponse();

        public bool IsValid { get => this.Statement.IsValid; }
        public string ParseDiagnostic { get => this.Statement.ParseDiagnostic; }
        public List<string> Errors { get => this.Statement.Errors; }
        public List<string> Warnings { get => this.Statement.Warnings; }
        public Dictionary<string, string> Disposition { get => this.Statement.Disposition; }
        protected QExplicitResult(QExplicitCommand command, QStatement stmt, string path, QCommandCategory category)
        {
            this.Category = category;
            this.FilePath = path;
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
        protected static QExplicitResult Create(QPrint print, QStatement stmt)
        {
            string path = string.Empty;
            return new QOutputResult(print, stmt, path, QCommandPoutFileMode.CREATE);
        }
        protected static QExplicitResult Create(QHistory history, QStatement stmt)
        {
            return new QHistoryResult(history, stmt, string.Empty);
        }
        public static QExplicitResult? Create(QExplicitCommand command, QStatement stmt)
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
            if (command.GetType() == typeof(QPrint))
                return Create((QPrint)command, stmt);
            if (command.GetType() == typeof(QHistory))
                return Create((QHistory)command, stmt);
 //         if (command.GetType() == typeof(QHistoryResult))
 //             return Create((QHistoryResult)command, stmt);

            return null;
        }
    }
    public class QOutputResult: QExplicitResult
    {
        QCommandPoutFileMode OutputFileMode;
        public QOutputResult(QExplicitCommand command, QStatement stmt, string path, QCommandPoutFileMode mode) : base(command, stmt, path, QCommandCategory.OUTPUT)
        {
            this.OutputFileMode = mode;
        }
        public override string GetResponse()
        {
            return this.IsValid ? "ok" : "error";
        }
    }
    public class QLabelResult : QExplicitResult
    {
        string Label;

        public QLabelResult(QExplicitCommand command, QStatement stmt, string path) : base(command, stmt, path, QCommandCategory.LABELING)
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
                    if (File.Exists(this.FilePath))
                    {
                        string yaml = File.ReadAllText(this.FilePath);
                        return "yaml";
                    }
                    return "error: Could not find the specified label.";
                }
                else return "ok";
            }
            return "error";
        }
    }
    public class QHistoryResult : QExplicitResult
    {
        public QHistoryResult(QExplicitCommand command, QStatement stmt, string path) : base(command, stmt, path, QCommandCategory.HISTORY)
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
        public QControlsResult(QExplicitCommand command, QStatement stmt, string path) : base(command, stmt, path, QCommandCategory.CONTROLS)
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
                string[] keys = new string[] { "span", "lexicon", "display", "format", "similarity", "version" };
                string[] vals = new string[6];
                for (int i = 0; i < vals.Length; i++)
                {
                    switch (i)
                    {
                        case 0: vals[i] = QSpan.DEFAULT.ToString(); break;
                        case 1: vals[i] = QLexicalDomain.DEFAULT.ToString(); break;
                        case 2: vals[i] = QLexicalDisplay.DEFAULT.ToString(); break;
                        case 3: vals[i] = QFormat.DEFAULT.ToString(); break;
                        case 4: vals[i] = QSimilarity.DEFAULT.ToString(); break;
                        case 5: vals[i] = version[0] + "." + version[1] + "." + version.Substring(2); break;
                    }
                }
                if (File.Exists(this.FilePath))
                {
                    string[] lines = File.ReadAllLines(this.FilePath);
                    foreach (string line in lines)
                    {
                        var parts = line.Split(':', 2);
                        if (parts.Length == 2)
                        {
                            string key = parts[0].Trim();
                            string val = parts[1].Trim();

                            for (int i = 0; i < keys.Length - 1 && i < vals.Length - 1; i++)
                            {
                                if (key.Equals(vals[i], StringComparison.InvariantCultureIgnoreCase))
                                {
                                    vals[i] = val; break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    return "error";
                }
                for (int i = 0; i < vals.Length; i++)
                {
                    if (vals[i].Equals(this.Setting))
                    {
                        vals[i] = "**" + vals[i] + "**";
                        break;
                    }
                }
                string table = @"| Setting | Meaning | Value |
| ---------- | ------------------------------------------------------------ | ------------ |
| {0}        | proximity distance limit                                     | {1}   |
| {2}        | the lexicon to be used for the searching                     | {3}   |
| {4}        | the lexicon to be used for display / rendering               | {5}   |
| {6}        | format of results on output (e.g. for exported results)      | {7}   |
| {8}        | fuzzy phonetics matching threshold is between 1 and 99 < br /> 0 or * none * means: do not match on phonetics(use text only) < br /> 100 or* exact*means that an *exact * phonetics match is expected | {9} |
| {10}       | revision number of the grammar. This value is read-only.     | {11}   |";
                string markdown = string.Format(table, keys[0], vals[0], keys[1], vals[1], keys[2], vals[2], keys[3], vals[3], keys[4], vals[4], keys[5], vals[5]);

                return markdown;
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
        public QSystemResult(QExplicitCommand command, QStatement stmt, string path) : base(command, stmt, path, QCommandCategory.SYSTEM)
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
