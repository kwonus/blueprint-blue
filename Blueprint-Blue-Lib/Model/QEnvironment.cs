namespace Blueprint.Blue
{
    public class QEnvironment
    {
        public string Format { get; set; }
        public string Scope { get; set; }
        public string User { get; set; }
        public string Session { get; set; }

        public QEnvironment()
        {
            this.Format = string.Empty;
            this.Scope = string.Empty;
            this.User = string.Empty;
            this.Session = string.Empty;
        }
    }
}