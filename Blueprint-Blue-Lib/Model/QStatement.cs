namespace Blueprint.Blue
{
    public class QStatement
    {
        public string Text { get; set; }
        public bool IsValid { get; set; }
        public string ParseDiagnostic { get; set; }
        public List<string> Errors { get; set; }
        public List<string> Warnings { get; set; }
        public QExplicitCommand? Singleton { get; set; }
        public QImplicitCommands? Commands { get; set; }
    }
}