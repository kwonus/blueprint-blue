namespace Blueprint.Blue
{
    using AVSearch.Model.Features;
    using Pinshot.PEG;

    public class QDelta : FeatureDelta
    {
        public QDelta(QFind search, string text, Parsed parse, bool negate) : base(text, negate)
        {
            this.hasDelta = false;
        }
    }
}