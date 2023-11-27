namespace Blueprint.Blue
{
    using Pinshot.PEG;
    using System.Collections.Generic;
    using XBlueprintBlue;

    public class QDecoration : QFeature, IFeature
    {
        public byte Decoration { get; set; }

        public QDecoration(QFind search, string text, Parsed parse, bool negate) : base(search, text, parse, negate)
        {
            this.Decoration = 0;
        }
        public override XFeature AsMessage()
        {
            var decor = new XPunctuation() { Bits = this.Decoration };
            var compare = new XCompare(decor);
            var feature = new XFeature { Feature = this.Text, Negate = false, Rule = "decoration", Match = compare };

            return feature;
        }
    }
}