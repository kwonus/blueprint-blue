namespace Blueprint.Blue
{
    using Pinshot.PEG;
    public class QStrongs : QFeature, IFeature
    {
        public int LemmaKey { get; set; }

        public QStrongs(QFind search, string text, Parsed parse) : base(search, text, parse)
        {
            ;
        }
    }
}
