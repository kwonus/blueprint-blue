namespace Blueprint.Blue
{
    using Pinshot.PEG;
    public class QLemma : QFeature, IFeature
    {
        public UInt16[] Lemmata { get; private set; }

        public QLemma(QFind search, string text, Parsed parse) : base(search, text, parse)
        {
            var oov = AVXLib.Framework.OOV.GetReverseEntry(text.ToLower());

            if (oov > 0)
            {
                this.Lemmata = new ushort[] { oov };
                return;  // If it is OOV, we can infer that this is a lemma
            }

            UInt16 key = 0;
            var record = QContext.Ortho.GetReverseLexRecord(text);
            if (record.found)
            {
                key = record.key;
            }
            else
            {
                // TO DO: If NOT EXACT:
                record = QContext.Ortho.GetReverseLexRecordModern(text);
                if (record.found)
                {
                    key = record.key;
                }
            }
            if (key > 0)
            {
                this.Lemmata = AVXLib.Framework.Lemmata.FindLemmataUsingWordKey(key);
            }
            else
            {
                this.Lemmata = new UInt16[0];
            }
        }
    }
}