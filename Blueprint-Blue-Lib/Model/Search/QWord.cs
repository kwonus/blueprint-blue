namespace Blueprint.Blue
{
    using AVXLib;
    using PhonemeEmbeddings;
    using Pinshot.PEG;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using XBlueprintBlue;

    public class QWord : QFeature, IFeature
    {
        public UInt16[] WordKeys { get; set; }
        public HashSet<string> Phonetic { get; private set; }
        private string Text;

        private bool AddPhonetics()
        {
            if (this.WordKeys != null)
            {
                foreach (var key in this.WordKeys)
                {
                    var modern = ObjectTable.AVXObjects.lexicon.GetLexModern(key);
                    var phone = new NUPhoneGen(modern);
                    if (!this.Phonetic.Contains(phone.Phonetic))
                        this.Phonetic.Add(phone.Phonetic);
                }
                return this.WordKeys.Length > 0;
            }
            return false;
        }
        private void AddRawPhonetics(string word)
        {
            var phone = new NUPhoneGen(word);
            if (!this.Phonetic.Contains(phone.Phonetic))
                this.Phonetic.Add(phone.Phonetic);
        }

        public QWord(QFind search, string text, Parsed parse, bool negate) : base(search, text, parse, negate)
        {
            this.Phonetic = new();

            var wkey = ObjectTable.AVXObjects.lexicon.GetReverseLex(text);
            this.WordKeys = (wkey != 0) ? new UInt16[1] { wkey } : new UInt16[0];
            if (!AddPhonetics())
                AddRawPhonetics(text);

            this.Text = text;
            if (this.WordKeys.Length == 0)
            {
                this.Search.Context.AddWarning("'" + text + "' is not in the lexicon (only sounds-alike searching can be used to match this token).");
            }
        }
        public override IEnumerable<string> AsYaml()
        {
            yield return "- feature: " + this.Text;
            string delimiter = "";
            var result = new StringBuilder("  wkeys: [ ", 48);
            foreach (var word in this.WordKeys)
            {
                if (delimiter.Length > 0)
                    result.Append(delimiter);
                else
                    delimiter = ", ";

                result.Append(word.ToString());
            }
            yield return (delimiter.Length > 0) ? result.ToString() + " ]" : "";
        }
        public override XFeature AsMessage()
        {
            var nuphone = this.Text.Length > 0 ? (new NUPhoneGen(this.Text)).Phonetic : string.Empty;
            var nuphones = (nuphone.Length > 0) ? new List<string>() { nuphone } : new List<string>();

            var lexes = new List<XLex>();
            foreach (var key in this.WordKeys)
            {
                if (nuphone.Length > 0)
                {
                    lexes.Add(new XLex() { Key = key, Variants = nuphones });
                }
                else
                {
                    lexes.Add(new XLex() { Key = key });
                }
            }
            var word = new XWord() { Lex = lexes };
            var compare = new XCompare(word);
            var feature = new XFeature { Feature = this.Text, Negate = false, Rule = "word", Match = compare };

            return feature;
        }
    }
}