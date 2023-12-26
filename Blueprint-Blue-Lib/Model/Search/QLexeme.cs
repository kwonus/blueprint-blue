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
        public UInt16[] WordKeys { get; private set; }
        public HashSet<string> Phonetics { get; private set; }

        public QWildcard? Wildcard { get; private set; }

        private bool AddPhonetics()
        {
            if (this.WordKeys != null && this.Wildcard == null)
            {
                foreach (var key in this.WordKeys)
                {
                    var modern = ObjectTable.AVXObjects.lexicon.GetLexModern(key);
                    var phone = new NUPhoneGen(modern);
                    if (!this.Phonetics.Contains(phone.Phonetic))
                        this.Phonetics.Add(phone.Phonetic);
                }
                return this.WordKeys.Length > 0;
            }
            return false;
        }
        private void AddRawPhonetics(string word)
        {
            var phone = new NUPhoneGen(word);
            if (!this.Phonetics.Contains(phone.Phonetic))
                this.Phonetics.Add(phone.Phonetic);
        }

        public QLexeme(QFind search, string text, Parsed parse, bool negate) : base(text, negate)
        {
            this.Phonetics = new();

            bool wildcard = parse.rule.Equals("wildcard", StringComparison.InvariantCultureIgnoreCase);

            if (wildcard)
            {
                this.Wildcard = new QWildcard(text, parse);
                this.WordKeys = this.Wildcard.GetLexemes(search.Settings);
            }
            else
            {
                var wkey = ObjectTable.AVXObjects.lexicon.GetReverseLex(text);
                this.WordKeys = (wkey != 0) ? new UInt16[1] { wkey } : new UInt16[0];
                if (!AddPhonetics())
                    AddRawPhonetics(text);

                if (this.WordKeys.Length == 0)
                {
                    search.AddWarning("'" + text + "' is not in the lexicon (only sounds-alike searching can be used to match this token).");
                }

                if (search.Settings.SearchSimilarity != (byte)SIMILARITY.NONE)
                {
                    ; // TO DO: similarity assessment
                }
            }
        }
    }
}