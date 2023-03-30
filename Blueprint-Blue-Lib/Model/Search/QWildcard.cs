namespace Blueprint.Blue
{
    using Pinshot.PEG;
    public class QWildcard : QFeature, IFeature
    {
        public string Beginning { get; set; }
        public string Ending { get; set; }

        public QWildcard(QFind search, string text, Parsed parse) : base(search, text, parse)
        {
            this.Beginning = string.Empty;
            this.Ending = string.Empty;
        }
    }
}