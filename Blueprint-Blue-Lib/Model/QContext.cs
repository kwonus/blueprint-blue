namespace Blueprint.Blue
{
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

        public Path? History { get; private set; }
        public Path? Macros { get; private set; }

        public QContext(IStatement statement, string session)
        {
            this.Statement = statement;

            this.Format  = string.Empty;
            this.Domain  = string.Empty;
            this.Span    = 0;     // zero means span is scoped by verse
            this.Exact   = false;
            this.User    = string.Empty;
            this.Session = string.Empty; // If session is a valid folder-path, then macros and history can be found here.
            this.Fields  = null;  // Null means that no fields were provided; In Quelle, this is different than an empty array of fields
        }
        public void AddError(string message)
        {
            this.Statement.AddError(message);
        }
        public void AddWarning(string message)
        {
            this.Statement.AddWarning(message);
        }
    }
}