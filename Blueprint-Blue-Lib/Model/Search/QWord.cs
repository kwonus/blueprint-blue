namespace Blueprint.Blue
{
    using Pinshot.PEG;

    public class QWord : QFeature, IFeature
    {
        public int WordKey { get; set; }

        public QWord(QFind search, string text, Parsed parse) : base(search, text, parse)
        {
            ;
        }
    }
}