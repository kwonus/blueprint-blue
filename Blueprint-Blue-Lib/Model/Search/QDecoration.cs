using Pinshot.PEG;

namespace Blueprint.Blue
{
    public class QDecoration : QTerm, ITerm
    {
        public int Decoration { get; set; }

        public QDecoration(string text, Parsed parse) : base(text, parse)
        {
            this.Decoration = 0;
        }
    }
}