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
                        string modern = ObjectTable.AVXObjects.lexicon.GetLexModern(lemma);
                        this.AddGeneratedNUPhone(modern);
                    }
                }
                return true;
            }
            return false;
        }
        private void AddGeneratedNUPhone(string word)
        {
            byte threshold = this.Settings.SearchSimilarity;

            if (threshold > 0 && threshold*10 <= FeatureGeneric.FullMatch)
            {
                var nuphone = new NUPhoneGen(word);
                if (!this.Phonetics.ContainsKey(nuphone.Phonetic))
                {
                    Dictionary<UInt16, UInt16> matches = new();
                    this.Phonetics[nuphone.Phonetic] = matches;

                    foreach (UInt16 key in ObjectTable.AVXObjects.Mem.Phonetics.Keys)
                    {
                        string payload = ObjectTable.AVXObjects.Mem.Phonetics[key].ToString();
                        string[] items = payload.Split('/', StringSplitOptions.RemoveEmptyEntries);

                        // NUPhone scoring is 1 to 10000
                        // scoring HERE is 1 to 1000
                        // threshold score is 33 to 100
                        foreach (string ipa in items)
                        {
                            NUPhoneGen candidate = new(ipa);
                            UInt16? score = nuphone.Compare(candidate, threshold);
                            if (score != null && score.Value >= threshold*100 && score.Value <= FeatureGeneric.FullMatch*10)
                            {
                                matches[key] = (UInt16) (score.Value / 10);
                            }
                        }
                    }
                }
            }
        }
        public QLemma(QFind search, string text, Parsed parse, bool negate) : base(text, negate, search.Settings)
        {
            this.Lemmata = new();
            this.Phonetics = new();

            var normalized = text.ToLower();
            if (this.Settings.EnableFuzzyLemmata)
            {
                AddGeneratedNUPhone(normalized);
            }

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