namespace Blueprint.Blue
{
    using Pinshot.PEG;
    using System.Collections.Generic;
    using XBlueprintBlue;

    public class QDecoration : QFeature, IFeature
    {
        public byte Decoration { get; set; }

        public QDecoration(QFind search, string text, Parsed parse) : base(search, text, parse)
        {
            this.Decoration = 0;
        }
        public override IEnumerable<string> AsYaml()
        {
            yield return "- feature: " + this.Text;
            yield return "  decoration: 0x" + this.Decoration.ToString("X");
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