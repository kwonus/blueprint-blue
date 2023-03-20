namespace Blueprint.Blue
{
    public class QImplicitCommands
    {
        public QEnvironment Environment { get; set; }
        public string ExpandedText { get; set; }
        public List<QImplicitCommand> Parts { get; set; }
        public List<QImplicitCommand> ExpandedParts { get; set; }

        public IEnumerable<QFind> Searches
        {
            get
            {
                foreach (var candidate in this.ExpandedParts)
                    if (candidate.GetType() == typeof(QFind))
                        yield return (QFind)candidate;
            }
        }
        public IEnumerable<QFilter> Filters
        {
            get
            {
                foreach (var candidate in this.ExpandedParts)
                    if (candidate.GetType() == typeof(QFilter))
                        yield return (QFilter)candidate;
            }
        }
        public IEnumerable<QVariable> Assignments
        {
            get
            {
                foreach (var candidate in this.ExpandedParts)
                    if (candidate.GetType() == typeof(QSet))
                        yield return (QVariable)candidate;
                    else if (candidate.GetType() == typeof(QClear))
                        yield return (QVariable)candidate;
            }
        }
        public QMacro? Macro
        {
            get
            {
                int cnt = 0;
                foreach (var candidate in this.ExpandedParts)
                    if (candidate.GetType() == typeof(QMacro))
                        cnt++;
                if (cnt == 1)
                    foreach (var candidate in this.ExpandedParts)
                        if (candidate.GetType() == typeof(QMacro))
                            return (QMacro)candidate;
                return null;
            }
        }
        public QExport? Export
        {
            get
            {
                int cnt = 0;
                foreach (var candidate in this.ExpandedParts)
                    if (candidate.GetType() == typeof(QExport))
                        cnt++;
                if (cnt == 1)
                    foreach (var candidate in this.ExpandedParts)
                        if (candidate.GetType() == typeof(QExport))
                            return (QExport)candidate;
                return null;
            }
        }
        public QDisplay? Display
        {
            get
            {
                int cnt = 0;
                foreach (var candidate in this.ExpandedParts)
                    if (candidate.GetType() == typeof(QDisplay))
                        cnt++;
                if (cnt == 1)
                    foreach (var candidate in this.ExpandedParts)
                        if (candidate.GetType() == typeof(QDisplay))
                            return (QDisplay)candidate;
                return null;
            }
        }
        public QImplicitCommands(QEnvironment env, string stmtText)
        {
            this.Environment = new QEnvironment();
            this.ExpandedText = stmtText;
            this.Parts = new List<QImplicitCommand>();
            this.ExpandedParts = new List<QImplicitCommand>();
        }
    }
}
