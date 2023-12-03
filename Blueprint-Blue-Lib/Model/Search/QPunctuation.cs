namespace Blueprint.Blue
{
    using Pinshot.PEG;
    using System.Collections.Generic;
    public class QPunctuation : QFeature, IFeature
    {
        override public string Type { get => QFeature.GetTypeName(this); }
        public byte Punctuation { get; set; }

        public QPunctuation(QFind search, string text, Parsed parse, bool negate) : base(search, text, parse, negate)
        {
            this.Punctuation = 0;
        }
    }
}