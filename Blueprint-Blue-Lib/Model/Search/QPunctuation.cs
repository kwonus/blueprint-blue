namespace Blueprint.Blue
{
    using Pinshot.PEG;
    public class QPunctuation : QFeature, IFeature
    {
        public int Punctuation { get; set; }

        public QPunctuation(QFind search, string text, Parsed parse) : base(search, text, parse)
        {
            this.Punctuation = 0;
        }
        public override IEnumerable<string> AsYaml()
        {
            yield return "- feature: " + this.Text;
            yield return "  punctuation: 0x" + this.Punctuation.ToString("X");
        }
    }
}