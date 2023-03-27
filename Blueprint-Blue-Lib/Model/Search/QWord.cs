using Pinshot.PEG;

namespace Blueprint.Blue
{
    public class QWord : QTerm, ITerm
    {
        public int WordKey { get; set; }

        public QWord(string text, Parsed parse) : base(text, parse)
        {
            ;
        }
    }
}