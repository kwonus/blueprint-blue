using Pinshot.PEG;

namespace Blueprint.Blue
{
    public class QPositional : QTerm, ITerm
    {
        public int Position { get; set; }

        public QPositional(string text, Parsed parse) : base(text, parse)
        {
            this.Position = 0;
        }
    }
}