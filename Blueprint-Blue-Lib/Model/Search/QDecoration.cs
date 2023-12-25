namespace Blueprint.Blue
{
    using AVSearch.Model.Features;
    using Pinshot.PEG;

    public class QDecoration : FeatureDecoration
    {
        public byte Decoration { get; private set; }

        public QDecoration(QFind search, string text, Parsed parse, bool negate) : base(text, negate)
        {
            this.Decoration = 0;
        }
    }
}