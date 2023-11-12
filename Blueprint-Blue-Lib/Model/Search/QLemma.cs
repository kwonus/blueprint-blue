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
    using AVXLib.Memory;

    public class QLemma : QFeature, IFeature
    {
        public UInt16[] Lemmata { get; private set; }
        public HashSet<string> Phonetics { get; private set; }

        private bool AddLemmata(UInt16[] lemmata)
        {
            if (lemmata != null)
            {
                this.Lemmata = lemmata;
                foreach (var key in lemmata)
                {
                    var modern = ObjectTable.AVXObjects.lexicon.GetLexModern(key);
                    var phone = new NUPhoneGen(modern);
                    if (!this.Phonetics.Contains(phone.Phonetic))
                        this.Phonetics.Add(phone.Phonetic);
                }
                return true;
            }
            this.Lemmata = new UInt16[0];
            return false;
        }
        private void AddRawPhonetics(string word)
        {
            this.Lemmata = new UInt16[0];

            var phone = new NUPhoneGen(word);
            if (!this.Phonetics.Contains(phone.Phonetic))
                this.Phonetics.Add(phone.Phonetic);
        }
        public QLemma(QFind search, string text, Parsed parse, bool negate) : base(search, text, parse, negate)
        {
            this.Phonetics = new();

            var normalized = text.ToLower();
            AddRawPhonetics(normalized);

            var lex = ObjectTable.AVXObjects.lexicon.GetReverseLexExtensive(normalized)[0];

            if (lex > 0)
            {
                var lemmas = ObjectTable.AVXObjects.lemmata.FindLemmataUsingWordKey(lex);
                if (AddLemmata(lemmas)) 
                {
                    return;  // If it is OOV, we can infer that this is a lemma
                }
                lemmas = ObjectTable.AVXObjects.lemmata.FindLemmataInList(lex);

                if (AddLemmata(lemmas))
                {
                    return;  // If it is OOV, we can infer that this is a lemma
                }
                this.Lemmata = new UInt16[0];
            }
            var oov = ObjectTable.AVXObjects.oov.GetReverseEntry(normalized);

            if (oov != 0)
            {
                var lemmas = ObjectTable.AVXObjects.lemmata.FindLemmataUsingWordKey(oov);

                if (AddLemmata(lemmas))
                {
                    return;  // If it is OOV, we can infer that this is a lemma
                }

                lemmas = ObjectTable.AVXObjects.lemmata.FindLemmataInList(oov);

                if (AddLemmata(lemmas))
                {
                    return;  // If it is OOV, we can infer that this is a lemma
                }
            }
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
            if (delimiter.Length > 0)
                yield return result.ToString() + " ]";

            result.Clear();
            delimiter = "\"";
            result.Append("  phonetics: [ ");
            foreach (var phonetic in this.Phonetics)
            {
                result.Append(delimiter);
                if (delimiter.Length > 1)
                    delimiter = "\", \"";

                result.Append(phonetic.Trim());
            }
            if (delimiter.Length > 0)
                yield return result.ToString() + "\" ]";
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
                if (this.Phonetics.Count > 0)
                {
                    lexes.Add(new XLex() { Key = key, Variants = this.Phonetics.ToList() });
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