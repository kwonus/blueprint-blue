namespace Blueprint.Blue
{
    using Pinshot.PEG;

    public class QDelta : QFeature, IFeature
    {
        override public string Type { get => QFeature.GetTypeName(this); }
        public bool hasDelta { get; set; }

        public QDelta(QFind search, string text, Parsed parse, bool negate) : base(search, text, parse, negate)
        {
            this.hasDelta = false;
        }
    }
}