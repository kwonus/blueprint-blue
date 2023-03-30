namespace Blueprint.Blue
{
    using Pinshot.PEG;
    public class QLemma : QFeature, IFeature
    {
        public int LemmaKey { get; set; }

        public QLemma(QFind search, string text, Parsed parse) : base(search, text, parse)
        {
            ;
        }
    }
}