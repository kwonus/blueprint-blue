namespace Blueprint.Blue
{
    using Pinshot.PEG;
    public class QLemma : QFeature, IFeature
    {
        public UInt16[] Lemmata { get; private set; }

        public QLemma(QFind search, string text, Parsed parse) : base(search, text, parse)
        {
            var normalized = text.ToLower();
            var lex = QContext.AVXObjects.written.GetReverseLexExtensive(normalized);

            if (lex > 0)
            {
                var lemmas = QContext.AVXObjects.lemmata.FindLemmataUsingWordKey(lex);

                if (lemmas != null) 
                {
                    this.Lemmata = lemmas;
                    return;  // If it is OOV, we can infer that this is a lemma
                }

                lemmas = QContext.AVXObjects.lemmata.FindLemmataInList(lex);

                if (lemmas != null)
                {
                    this.Lemmata = lemmas;
                    return;  // If it is OOV, we can infer that this is a lemma
                }
                this.Lemmata = new UInt16[0];
            }
            var oov = QContext.AVXObjects.oov.GetReverseEntry(normalized);

            if (oov != 0)
            {
                var lemmas = QContext.AVXObjects.lemmata.FindLemmataUsingWordKey(oov);

                if (lemmas != null)
                {
                    this.Lemmata = lemmas;
                    return;  // If it is OOV, we can infer that this is a lemma
                }

                lemmas = QContext.AVXObjects.lemmata.FindLemmataInList(oov);

                if (lemmas != null)
                {
                    this.Lemmata = lemmas;
                    return;  // If it is OOV, we can infer that this is a lemma
                }
            }
            this.Lemmata = new UInt16[0];
        }
    }
}