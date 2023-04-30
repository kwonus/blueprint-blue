namespace Blueprint.Blue
{
    using Pinshot.PEG;
    using XBlueprintBlue;
    using System.Text;
    using System;
    using System.Collections.Generic;

    public class QLemma : QFeature, IFeature
    {
        public UInt16[] Lemmata { get; private set; }

        public QLemma(QFind search, string text, Parsed parse) : base(search, text, parse)
        {
            var normalized = text.ToLower();
            var lex = QContext.AVXObjects.written.GetReverseLexExtensive(normalized)[0];

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
        public override IEnumerable<string> AsYaml()
        {
            yield return "- feature: " + this.Text;
            string delimiter = "";
            var result = new StringBuilder("  lemmata: [ ", 48);
            foreach (var lemma in this.Lemmata)
            {
                if (delimiter.Length > 0)
                    result.Append(delimiter);
                else
                    delimiter = ", ";

                result.Append(lemma.ToString());
            }
            yield return (delimiter.Length > 0) ? result.ToString() + " ]" : "";
        }
        public override XFeature AsMessage()
        {
            var lemmata = new List<UInt16>(this.Lemmata);
            var lemma = new XLemma() { Lemmata = lemmata };
            var compare = new XCompare(lemma);
            var feature = new XFeature { Feature = this.Text, Negate = false, Rule = "lemmata", Match = compare };

            return feature;
        }
    }
}