namespace Blueprint.Blue
{
    using Pinshot.PEG;

    public class QTransition : QFeature, IFeature
    {
        override public string Type { get => QFeature.GetTypeName(this); }
        public byte Transition { get; set; }

        public QTransition(QFind search, string text, Parsed parse, bool negate) : base(search, text, parse, negate)
        {
            this.Transition = 0;
        }
    }
}