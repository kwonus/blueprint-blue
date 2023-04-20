namespace Blueprint.Blue
{
    using Pinshot.PEG;
    using System.Text;

    public class QWord : QFeature, IFeature
    {
        public UInt16[] WordKeys { get; set; }

        public QWord(QFind search, string text, Parsed parse) : base(search, text, parse)
        {
            this.WordKeys = QContext.AVXObjects.written.GetReverseLexExtensive(text, this.Search.Context.LocalSettings.Exact.Value);
            if (this.WordKeys.Length == 0)
            {
                this.Search.Context.AddError("A word was specified that could not be found in the lexicon: " + text);
            }
        }
        public override IEnumerable<string> AsYaml()
        {
            yield return "- feature: " + this.Text;
            string delimiter = "";
            var result = new StringBuilder("  wkeys: [ ", 48);
            foreach (var word in this.WordKeys)
            {
                if (delimiter.Length > 0)
                    result.Append(delimiter);
                else
                    delimiter = ", ";

                result.Append(word.ToString());
            }
            yield return (delimiter.Length > 0) ? result.ToString() + " ]" : "";
        }
        public override XFeature AsMessage()
        {
            var keys = new List<UInt16>(this.WordKeys);
            var word = new XWord() { Wkeys = keys };
            var compare = new XCompare(word);
            var feature = new XFeature { Feature = this.Text, Negate = false, Rule = "word", Match = compare };

            return feature;
        }
    }
}