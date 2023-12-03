namespace Blueprint.Blue
{
    using Pinshot.PEG;
    using System.Text;
    using System;
    using System.Collections.Generic;
    using PhonemeEmbeddings;
    using AVXLib.Framework;
    using AVXLib;
    using AVXLib.Memory;
    using YamlDotNet.Serialization;

    public class QLemma : QFeature, IFeature
    {
        override public string Type { get => QFeature.GetTypeName(this); }
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
    }
}