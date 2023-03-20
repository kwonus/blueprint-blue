namespace Blueprint.Blue
{
    public class QPunctuation : QTerm, ITerm
    {
        public int Punctuation { get; set; }

        public QPunctuation(string text) : base(text)
        {
            this.Punctuation = 0;
        }
    }
}