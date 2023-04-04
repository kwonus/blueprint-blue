namespace Blueprint.Blue
{
    using AVXLib.Framework;
    using BlueprintBlue;
    using System.Linq;

    public class QContext: IStatement
    {
        public string   Format { get; set; }
        public string   Domain { get; set; }
        public UInt16   Span   { get; set; }
        public bool     Exact  { get; set; }
        public string   User   { get; set; }
        public string   Session{ get; set; }
        public UInt16[]?Fields { get; set; }

        private IStatement Statement { get; set; }

        public string History { get; private set; }
        public string Macros { get; private set; }

        public static AVXLib.ObjectTable AVXObjects { get; internal set; } = AVXLib.ObjectTable.Create(@"C:\src\Digital-AV\omega\AVX-Omega.data");

        public QContext(IStatement statement, string session)
        {
            this.Statement = statement;

            this.Format  = string.Empty;
            this.Domain  = string.Empty;
            this.Span    = 0;     // zero means span is scoped by verse
            this.Exact   = false;
            this.User    = string.Empty;
            this.Session = session.Replace("\\", "/"); // always use unix-style path-spec
            this.Fields  = null;    // Null means that no fields were provided; In Quelle, this is different than an empty array of fields
            this.History = string.Empty;
            this.Macros  = string.Empty;

            if (!string.IsNullOrEmpty(this.Session))
            {
                if (Directory.Exists(this.Session)) // If session is a valid folder-path, then macros and history can be found here.
                {
                    var quelle = session.EndsWith("/Quelle", StringComparison.InvariantCultureIgnoreCase) || session.EndsWith("/Quelle/", StringComparison.InvariantCultureIgnoreCase);
                    var folder = quelle ? this.Session : Path.Combine(this.Session, "Quelle");

                    this.History = Path.Combine(folder, "History.Quelle").Replace("\\", "/");
                    this.Macros = Path.Combine(folder, "Labels").Replace("\\", "/");
                }
                else
                {
                    this.AddWarning("A session context was provided that does not represents a valid path");
                }
            }
            else
            {
                this.AddWarning("A session context has not been provided");
            }
            if (!QContext.AVXObjects.Mem.valid)
            {
                this.AddError("Unable to load AVX Data. Without this library, other things will break");
            }
        }
        public void AddError(string message)
        {
            this.Statement.AddError(message);
        }
        public void AddWarning(string message)
        {
            this.Statement.AddWarning(message);
        }
        public void AddHistory(string stmt)
        {
            var time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            uint seq = 0;
            try
            {
                var lastLine = File.ReadLines(this.History).Last();
                var parts = lastLine.Split('\t');
                seq = 1 + uint.Parse(parts[0]);
            }
            catch
            {
                this.AddWarning("Unable to parse History.quelle; Resetting history file");
                seq = 1;
            }
            try
            {
                using (StreamWriter sw = File.AppendText(this.History))
                {
                    string line = seq.ToString() + "\t" + time.ToString() + "\t" + stmt;
                    sw.WriteLine(line);
                }
            }
            catch
            {
                this.AddError("Unable to save history as requested");
            }
        }
        public Dictionary<uint, QHistory> GetHistory(uint minSeq = 0, uint maxSeq = uint.MaxValue, DateTime? notBefore = null, DateTime? notAfter = null)
        {
            var result = new Dictionary<uint, QHistory>();

            var notBeforeOffset = notBefore != null ? new DateTimeOffset(notBefore.Value) : DateTimeOffset.MinValue;
            var notAfterOffset  = notAfter  != null ? new DateTimeOffset(notAfter.Value)  : DateTimeOffset.MaxValue;

            var notBeforeUInt64 = notBeforeOffset.ToUnixTimeMilliseconds();
            var notAfterUInt64  = notAfterOffset.ToUnixTimeMilliseconds();

            try
            {
                var lines = File.ReadLines(this.History);
                foreach (string line in lines)
                {
                    var parts = line.Split('\t');
                    if (parts.Length == 3)
                    {
                        var seq = uint.Parse(parts[0]);
                        var time = Int64.Parse(parts[1]);

                        var item = new QHistory() { Sequence = seq, Time = time, Statement = parts[3] };
                    }
                    else
                    {
                        this.AddError("An entry in History.quelle was unreadable");
                    }
                }
            }
            catch
            {
                this.AddError("Unable to read History.quelle");
            }
            return result;
        }
        public QHistory? GetHistoryEntry(uint sequence)
        {
            if (sequence > 0 && sequence < uint.MaxValue)
            {
                var history = this.GetHistory(minSeq: sequence, maxSeq: sequence);

                if (history.Count == 1)
                    return history[sequence];
            }
            return null;
        }
        public void AddMacro(string stmt, string label)
        {
            var macro = Path.Combine(this.Macros, label + ".quelle");

            try
            {
                using (var file = new StreamWriter(File.Create(macro)))
                {
                    file.Write(stmt);
                }
            }
            catch
            {
                this.AddError("Cannot create macro: " + label);
            }
        }
        public string GetMacro(string label)
        {
            var macro = Path.Combine(this.Macros, label + ".quelle");

            try
            {
                using (var file = new StreamWriter(File.Create(macro)))
                {
                    string stmt = File.ReadAllText(this.History);
                    return stmt;
                }
            }
            catch
            {
                this.AddError("Cannot read macro: " + label);
            }
            return string.Empty;
        }
    }
}