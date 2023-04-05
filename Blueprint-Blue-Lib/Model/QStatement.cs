using BlueprintBlue;

namespace Blueprint.Blue
{
    public interface IStatement
    {
        void AddError(string message);
        void AddWarning(string message);
    }
    public class QStatement: IStatement
    {
        public string Text { get; set; }
        public bool IsValid { get; set; }
        public string ParseDiagnostic { get; set; }
        public List<string> Errors { get; set; }
        public List<string> Warnings { get; set; }
        public QExplicitCommand? Singleton { get; set; }
        public QImplicitCommands? Commands { get; set; }
        public QContext Context { get; private set; }

        public QStatement()
        {
            this.Text = string.Empty;
            this.IsValid= false;
            this.ParseDiagnostic= string.Empty;
            this.Errors = new();
            this.Warnings = new();
            this.Singleton = null;
            this.Commands= null;
            this.Context = new QContext(this, @"C:\Users\Me\AVX");  // notional placeholder for now (base this on actual username/home
        }

        public void AddError(string message)
        {
            this.Errors.Add(message);
        }

        public void AddWarning(string message)
        {
            this.Warnings.Add(message);
        }
        public QExpandableStatement? AddHistory()
        {
            return new QExpandableStatement(this);
        }
    }
}