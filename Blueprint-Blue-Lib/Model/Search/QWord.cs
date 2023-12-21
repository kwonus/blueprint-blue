namespace Blueprint.Blue
{
    using AVXLib;
    using PhonemeEmbeddings;
    using Pinshot.PEG;
    using System;
    using System.Collections.Generic;

    public class QWord : QFeature, IFeature
    {
        override public string Type { get => QFeature.GetTypeName(this); }
        public UInt16[] WordKeys { get; set; }
        public HashSet<string> Phonetics { get; private set; }

        private bool AddPhonetics()
        {
            if (this.WordKeys != null)
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

        public QWord(QFind search, string text, Parsed parse, bool negate) : base(search, text, parse, negate)
        {
            this.Phonetics = new();

            var wkey = ObjectTable.AVXObjects.lexicon.GetReverseLex(text);
            this.WordKeys = (wkey != 0) ? new UInt16[1] { wkey } : new UInt16[0];
            if (!AddPhonetics())
                AddRawPhonetics(text);

            if (this.WordKeys.Length == 0)
            {
                this.Search.AddWarning("'" + text + "' is not in the lexicon (only sounds-alike searching can be used to match this token).");
            }
        }
    }
}