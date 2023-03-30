namespace Blueprint.Blue
{
    using Pinshot.PEG;
    public class QTransition : QFeature, IFeature
    {
        public int Decoration { get; set; }

        public QTransition(QFind search, string text, Parsed parse) : base(search, text, parse)
        {
            this.Decoration = 0;
        }
    }
}