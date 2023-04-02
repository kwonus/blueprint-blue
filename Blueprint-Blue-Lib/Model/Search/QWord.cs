namespace Blueprint.Blue
{
    using Pinshot.PEG;

    public class QWord : QFeature, IFeature
    {
        public int WordKey { get; set; }

        public QWord(QFind search, string text, Parsed parse) : base(search, text, parse)
        {
            var record = QContext.Ortho.GetReverseLexRecord(text);
            if (record.found)
            {
                this.WordKey = record.key;
                return;
            }
            // TO DO: If NOT EXACT:
            record = QContext.Ortho.GetReverseLexRecordModern(text);
            if (record.found)
            {
                this.WordKey = record.key;
                return;
            }
            var dehyphenated = text.Replace("-", "");
            record = QContext.Ortho.GetReverseLexRecord(dehyphenated);
            if (record.found)
            {
                this.WordKey = record.key;
                return;
            }
            // TO DO: If NOT EXACT:
            record = QContext.Ortho.GetReverseLexRecordModern(dehyphenated);
            if (record.found)
            {
                this.WordKey = record.key;
                return;
            }
            this.WordKey = 0;
        }
    }
}