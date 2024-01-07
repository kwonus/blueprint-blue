namespace Blueprint.Blue
{
    using AVSearch.Model.Features;
    using Pinshot.PEG;

    public class QTransition : FeatureTransition
    {
        public QTransition(QFind search, string text, Parsed parse, bool negate) : base(text, negate)
        {
            this.Transition = 0;
        }
    }
}