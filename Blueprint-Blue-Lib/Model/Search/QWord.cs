namespace Blueprint.Blue
{
    using Pinshot.PEG;

    public class QWord : QFeature, IFeature
    {
        public int WordKey { get; set; }

        public QWord(QFind search, string text, Parsed parse) : base(search, text, parse)
        {
            var record = QContext.AVXObjects.written.GetReverseLexRecord(text);
            if (record.found)
            {
                this.WordKey = record.key;
                return;
            }
            record = QContext.AVXObjects.written.GetReverseLexRecordExtensive(text, this.Search.Context.LocalSettings.Exact.Value);
            if (record.found)
            {
                this.WordKey = record.key;
            }
            else
            {
                this.Search.Context.AddError("A word was specified that could not be found in the lexicon: " + text);
            }
        }
    }
}