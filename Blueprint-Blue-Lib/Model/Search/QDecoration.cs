namespace Blueprint.Blue
{
    using Pinshot.PEG;
    public class QDecoration : QFeature, IFeature
    {
        public int Decoration { get; set; }

        public QDecoration(QFind search, string text, Parsed parse) : base(search, text, parse)
        {
            this.Decoration = 0;
        }
    }
}