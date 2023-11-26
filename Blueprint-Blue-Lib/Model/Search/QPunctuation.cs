namespace Blueprint.Blue
{
    using Pinshot.PEG;
    using System.Collections.Generic;
    using XBlueprintBlue;
    public class QPunctuation : QFeature, IFeature
    {
        public byte Punctuation { get; set; }

        public QPunctuation(QFind search, string text, Parsed parse, bool negate) : base(search, text, parse, negate)
        {
            this.Punctuation = 0;
        }
        public override IEnumerable<string> AsYaml()
        {
            return ICommand.YamlSerializer(this);
        }
        public override XFeature AsMessage()
        {
            var decor = new XPunctuation() { Bits = this.Punctuation };
            var compare = new XCompare(decor);
            var feature = new XFeature { Feature = this.Text, Negate = false, Rule = "punctuation", Match = compare };

            return feature;
        }
    }
}