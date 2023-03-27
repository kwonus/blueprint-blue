using Pinshot.PEG;

namespace Blueprint.Blue
{
    public class QPunctuation : QTerm, ITerm
    {
        public int Punctuation { get; set; }

        public QPunctuation(string text, Parsed parse) : base(text, parse)
        {
            this.Punctuation = 0;
        }
    }
}