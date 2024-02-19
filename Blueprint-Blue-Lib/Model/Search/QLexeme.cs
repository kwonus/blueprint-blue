namespace Blueprint.Blue
{
    using AVSearch.Model.Features;
    using AVXLib;
    using Blueprint.Model.Implicit;
    using PhonemeEmbeddings;
    using Pinshot.PEG;
    using System;
    using System.Collections.Generic;
    using YamlDotNet.Serialization;
    using static System.Net.Mime.MediaTypeNames;

    public class QLexeme : FeatureLexeme
    {
        private bool AddPhonetics()
        {
            if (this.WordKeys != null && this.Wildcard == null && this.Settings.SearchSimilarity.lemma > 0)
            {
                foreach (var key in this.WordKeys)
                {
                    string modern = ObjectTable.AVXObjects.lexicon.GetLexModern(key, 0);
                    AddGeneratedNUPhone(modern);
                }
                return this.WordKeys.Count > 0;
            }
            return false;
        }
        private bool AddGeneratedNUPhone(string word, bool raw = false)
        {
            bool found = false;
            byte threshold = this.Settings.SearchSimilarity.word;

            if (threshold > 0 && threshold*10 <= FeatureGeneric.FullMatch)
            {
                var nuphone = new NUPhoneGen(word, raw);
                if (raw == true)
                    found = true;
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
                            if (score != null && score.Value >= threshold * 100 && score.Value <= FeatureGeneric.FullMatch * 10)
                            {
                                matches[key] = (UInt16)(score.Value / 10);
                                found = true;
                            }
                        }
                    }
                }
            }
            return found;
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
                this.WordKeys = AVXLib.Framework.Lexicon.GetReverseLex(text, this.Settings.SearchAsAV, this.Settings.SearchAsAVX);

                if (!AddPhonetics())
                {
                    AddGeneratedNUPhone(text);
                }
                if (this.WordKeys.Count == 0)
                {
                    search.AddWarning("'" + text + "' is not in the lexicon (only sounds-alike searching can be used to match this token).");
                }
            }
        }
    }
}