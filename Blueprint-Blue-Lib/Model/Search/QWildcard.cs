using Pinshot.PEG;

namespace Blueprint.Blue
{
    public class QWildcard : QTerm, ITerm
    {
        public string Beginning { get; set; }
        public string Ending { get; set; }

        public QWildcard(string text, Parsed parse) : base(text, parse)
        {
            this.Beginning = string.Empty;
            this.Ending = string.Empty;
        }
    }
}