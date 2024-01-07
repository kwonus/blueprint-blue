namespace Blueprint.Blue
{
    using Pinshot.PEG;
    using System;
    using System.Collections.Generic;
    using PhonemeEmbeddings;
    using AVXLib;
    using AVSearch.Model.Features;
    using AVSearch.Interfaces;

    public class QLemma : FeatureLemma
    {
         private bool AddLemmata(UInt16[] lemmata, ISettings settings)
         {
            this.Lemmata = new();
            if (lemmata != null)
            {
                foreach (UInt16 lemma in lemmata)
                {
                    if (this.Lemmata.Contains(lemma))
                        continue;

                    this.Lemmata.Add(lemma);

                    if (settings.EnableFuzzyLemmata)
                    {
                        var modern = ObjectTable.AVXObjects.lexicon.GetLexModern(lemma);
                        var phone = new NUPhoneGen(modern);
                        if (!this.Phonetics.Contains(phone.Phonetic))
                            this.Phonetics.Add(phone.Phonetic);
                    }
                }
                return true;
            }
            return false;
        }
        private void AddRawPhonetics(string word)
        {
            this.Lemmata = new();

            var phone = new NUPhoneGen(word);
            if (!this.Phonetics.Contains(phone.Phonetic))
                this.Phonetics.Add(phone.Phonetic);
        }
        public QLemma(QFind search, string text, Parsed parse, bool negate) : base(text, negate)
        {
            this.Lemmata = new();
            this.Phonetics = new();

            var normalized = text.ToLower();
            AddRawPhonetics(normalized);

            var lex = ObjectTable.AVXObjects.lexicon.GetReverseLexExtensive(normalized)[0];

            if (lex > 0)
            {
                var lemmas = ObjectTable.AVXObjects.lemmata.FindLemmataUsingWordKey(lex);
                if (AddLemmata(lemmas, search.Settings)) 
                {
                    return;  // If it is OOV, we can infer that this is a lemma
                }
                lemmas = ObjectTable.AVXObjects.lemmata.FindLemmataInList(lex);

                if (AddLemmata(lemmas, search.Settings))
                {
                    return;  // If it is OOV, we can infer that this is a lemma
                }
            }
            var oov = ObjectTable.AVXObjects.oov.GetReverseEntry(normalized);

            if (oov != 0)
            {
                var lemmas = ObjectTable.AVXObjects.lemmata.FindLemmataUsingWordKey(oov);

                if (AddLemmata(lemmas, search.Settings))
                {
                    return;  // If it is OOV, we can infer that this is a lemma
                }

                lemmas = ObjectTable.AVXObjects.lemmata.FindLemmataInList(oov);

                if (AddLemmata(lemmas, search.Settings))
                {
                    return;  // If it is OOV, we can infer that this is a lemma
                }
            }
        }
    }
}