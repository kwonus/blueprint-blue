namespace Blueprint.Blue
{
    using Pinshot.PEG;
    using XBlueprintBlue;
    using System.Text;
    using System;
    using System.Collections.Generic;
    using PhonemeEmbeddings;
    using AVXLib.Framework;
    using AVXLib;

    public class QLemma : QFeature, IFeature
    {
        public UInt16[] Lemmata { get; private set; }

        public QLemma(QFind search, string text, Parsed parse, bool negate) : base(search, text, parse, negate)
        {
            var normalized = text.ToLower();
            var lex = ObjectTable.AVXObjects.lexicon.GetReverseLexExtensive(normalized)[0];

            if (lex > 0)
            {
                var lemmas = ObjectTable.AVXObjects.lemmata.FindLemmataUsingWordKey(lex);

                if (lemmas != null) 
                {
                    this.Lemmata = lemmas;
                    return;  // If it is OOV, we can infer that this is a lemma
                }

                lemmas = ObjectTable.AVXObjects.lemmata.FindLemmataInList(lex);

                if (lemmas != null)
                {
                    this.Lemmata = lemmas;
                    return;  // If it is OOV, we can infer that this is a lemma
                }
                this.Lemmata = new UInt16[0];
            }
            var oov = ObjectTable.AVXObjects.oov.GetReverseEntry(normalized);

            if (oov != 0)
            {
                var lemmas = ObjectTable.AVXObjects.lemmata.FindLemmataUsingWordKey(oov);

                if (lemmas != null)
                {
                    this.Lemmata = lemmas;
                    return;  // If it is OOV, we can infer that this is a lemma
                }

                lemmas = ObjectTable.AVXObjects.lemmata.FindLemmataInList(oov);

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
            var lexes = new List<XLex>();
            foreach (var key in this.Lemmata)
            {
                var text = ObjectTable.AVXObjects.lexicon.GetLexNormalized(key);
                if (string.IsNullOrEmpty(text))
                {
                    var oov = ObjectTable.AVXObjects.oov.GetEntry(key);
                    if (oov.valid)
                        text = oov.oov.text.ToString();
                }
                var nuphone = text.Length > 0 ? (new NUPhoneGen(text)).Phonetic : string.Empty;
                if (nuphone.Length > 0)
                {
                    var nuphones = new List<string>() { nuphone };
                    lexes.Add(new XLex() { Key = key, Variants = nuphones });
                }
                else
                {
                    lexes.Add(new XLex() { Key = key });
                }
            }
            var lemma = new XLemma() { Lemmata = lexes };

            var compare = new XCompare(lemma);
            var feature = new XFeature { Feature = this.Text, Negate = false, Rule = "lemmata", Match = compare };

            return feature;
        }
    }
}