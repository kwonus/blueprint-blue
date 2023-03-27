using Pinshot.PEG;

namespace Blueprint.Blue
{
    public class QLemma : QTerm, ITerm
    {
        public int LemmaKey { get; set; }

        public QLemma(string text, Parsed parse) : base(text, parse)
        {
            ;
        }
    }
}