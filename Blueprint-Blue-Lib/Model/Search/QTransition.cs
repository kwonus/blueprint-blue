namespace Blueprint.Blue
{
    using Pinshot.PEG;
    using System.Collections.Generic;
    using XBlueprintBlue;
    public class QTransition : QFeature, IFeature
    {
        public byte Transition { get; set; }

        public QTransition(QFind search, string text, Parsed parse, bool negate) : base(search, text, parse, negate)
        {
            this.Transition = 0;
        }
        public override XFeature AsMessage()
        {
            var transit = new XTransition() { Bits = this.Transition };
            var compare = new XCompare(transit);
            var feature = new XFeature { Feature = this.Text, Negate = false, Rule = "decoration", Match = compare };

            return feature;
        }
    }
}