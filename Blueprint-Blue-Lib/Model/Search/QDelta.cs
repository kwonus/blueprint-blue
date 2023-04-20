namespace Blueprint.Blue
{
    using Pinshot.PEG;
    public class QDelta : QFeature, IFeature
    {
        public bool hasDelta { get; set; }

        public QDelta(QFind search, string text, Parsed parse) : base(search, text, parse)
        {
            this.hasDelta = false;
        }
        public override IEnumerable<string> AsYaml()
        {
            yield return string.Empty;  // this method is not wired in yet !!!!
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