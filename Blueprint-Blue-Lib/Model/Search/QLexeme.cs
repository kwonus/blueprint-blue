namespace Blueprint.Blue
{
    using AVSearch.Model.Features;
    using AVXLib;
    using Blueprint.Model.Implicit;
    using PhonemeEmbeddings;
    using Pinshot.PEG;
    using System;
    using System.Collections.Generic;

    public class QLexeme : FeatureLexeme
    {
        private bool AddPhonetics()
        {
            if (this.WordKeys != null && this.Wildcard == null && this.Settings.SearchSimilarity > 0)
            {
                foreach (var key in this.WordKeys)
                {
                    if (this.Settings.EnableFuzzyLemmata)
                    {
                        string modern = ObjectTable.AVXObjects.lexicon.GetLexModern(key);
                        AddGeneratedNUPhone(modern);
                    }
                }
                return this.WordKeys.Count > 0;
            }
            return false;
        }
        private void AddGeneratedNUPhone(string word, bool raw = false)
        {
            int threshold = this.Settings.SearchSimilarity;

            if (threshold > 0 && threshold <= FeatureGeneric.FullMatch / 100)
            {
                var nuphone = new NUPhoneGen(word, raw);
                if (!this.Phonetics.ContainsKey(nuphone.Phonetic))
                {
                    Dictionary<UInt16, UInt16> matches = new();
                    this.Phonetics[nuphone.Phonetic] = matches;

                    foreach (UInt16 key in ObjectTable.AVXObjects.Mem.Phonetics.Keys)
                    {
                        string payload = ObjectTable.AVXObjects.Mem.Phonetics[key].ToString();
                        string[] items = payload.Split('/', StringSplitOptions.RemoveEmptyEntries);

                        foreach (string ipa in items)
                        {
                            NUPhoneGen candidate = new(ipa);
                            UInt16 score = nuphone.Compare(candidate);
                            if (score >= threshold * 100 && score <= FeatureGeneric.FullMatch)
                            {
                                matches[key] = score;
                            }
                        }
                    }
                }
            }
        }
        private void AddRawNUPhone(string phonetic)
        {
            this.AddGeneratedNUPhone(phonetic, raw: true);
        }

        public QLexeme(QFind search, string text, Parsed parse, bool negate) : base(text, negate, search.Settings)
        {
            this.Phonetics = new();

            bool wildcard = parse.rule.Equals("wildcard", StringComparison.InvariantCultureIgnoreCase);
            bool phonetic = parse.rule.Equals("nuphone", StringComparison.InvariantCultureIgnoreCase);
            bool phonetic_wildcard = parse.rule.Equals("wildcard_phonetic", StringComparison.InvariantCultureIgnoreCase);

            if (wildcard || phonetic_wildcard)
            {
                this.Wildcard = new QWildcard(text, parse, phonetic_wildcard ? AVSearch.Model.Types.WildcardType.NuphoneTerm : AVSearch.Model.Types.WildcardType.EnglishTerm);
                this.WordKeys = this.Wildcard.GetLexemes(search.Settings);
            }
            else if (phonetic)
            {
                this.AddRawNUPhone(text);
            }
            else
            {
                var wkey = ObjectTable.AVXObjects.lexicon.GetReverseLex(text);
                this.WordKeys = new();
                if (wkey != 0)
                    this.WordKeys.Add(wkey);

                if (!AddPhonetics())
                    AddGeneratedNUPhone(text);

                if (this.WordKeys.Count == 0)
                {
                    search.AddWarning("'" + text + "' is not in the lexicon (only sounds-alike searching can be used to match this token).");
                }
            }
        }
    }
}