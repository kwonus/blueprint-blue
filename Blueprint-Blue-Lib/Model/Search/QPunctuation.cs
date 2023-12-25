namespace Blueprint.Blue
{
    using AVSearch.Model.Features;
    using Pinshot.PEG;
    public class QPunctuation : FeaturePunctuation
    {
        public byte Punctuation { get; private set; }

        public QPunctuation(QFind search, string text, Parsed parse, bool negate) : base(text, negate)
        {
            this.Punctuation = 0;
        }
    }
}