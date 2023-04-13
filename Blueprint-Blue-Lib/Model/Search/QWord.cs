namespace Blueprint.Blue
{
    using Pinshot.PEG;

    public class QWord : QFeature, IFeature
    {
        public UInt16 WordKey { get; set; }

        public QWord(QFind search, string text, Parsed parse) : base(search, text, parse)
        {
            this.WordKey = QContext.AVXObjects.written.GetReverseLexExtensive(text, this.Search.Context.LocalSettings.Exact.Value);
            if (this.WordKey == 0)
            {
                this.Search.Context.AddError("A word was specified that could not be found in the lexicon: " + text);
            }
        }
    }
}