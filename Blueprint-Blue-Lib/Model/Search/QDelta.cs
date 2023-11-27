namespace Blueprint.Blue
{
    using Pinshot.PEG;
    using System.Collections.Generic;
    using XBlueprintBlue;
    public class QDelta : QFeature, IFeature
    {
        public bool hasDelta { get; set; }

        public QDelta(QFind search, string text, Parsed parse, bool negate) : base(search, text, parse, negate)
        {
            this.hasDelta = false;
        }
        public override XFeature AsMessage()
        {
            var delta = new XDelta() { Differs = this.hasDelta };
            var compare = new XCompare(delta);
            var feature = new XFeature { Feature = this.Text, Negate = false, Rule = "decoration", Match = compare };

            return feature;
        }
    }
}