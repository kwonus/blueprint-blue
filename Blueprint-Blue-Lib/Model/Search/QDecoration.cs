namespace Blueprint.Blue
{
    using AVSearch.Model.Features;
    using Pinshot.PEG;

    public class QDecoration : FeatureDecoration
    {
        public QDecoration(QFind search, string text, Parsed parse, bool negate) : base(text, negate, search.Settings)
        {
            this.Decoration = 0;
        }
    }
}