namespace Blueprint.Blue
{
    using Pinshot.PEG;
    using System;
    using System.Text.Json.Serialization;
    using YamlDotNet.Serialization;

    public interface IFeature
    {
        string Type { get; }
        string Text { get; }
        bool Negate { get; }
    }

    public abstract class QFeature : IFeature // This is mostly redundant with QSearchFeature. The inheritance aspect of this class needs to be preserved 
    {
        abstract public string Type { get; }
        public string Text { get; private set; }
        public bool Negate { get; private set; }
        [JsonIgnore]
        [YamlIgnore]
        public Parsed Parse { get; private set; }
        [JsonIgnore]
        [YamlIgnore]
        public QFind Search { get; private set; }
        protected static string GetTypeName(object obj)
        {
            string name = obj.GetType().Name; // Always "Q*"

            return (name.Length >= 2 && name[0] == 'Q') ? name.Substring(1) : name;
        }
        protected QFeature(QFind context, string text, Parsed parse, bool negate)
        {
            this.Text = text.Trim();
            this.Parse = parse;
            this.Search = context;
            this.Negate = negate;

            if (this.Negate && this.Text.StartsWith('-'))
            {
                this.Text = this.Text.Length > 1 ? this.Text.Substring(1) : string.Empty;
            }
        }
        public static QFeature? Create(QFind search, string text, Parsed parse)
        {
            if (parse.rule.Equals("feature", StringComparison.InvariantCultureIgnoreCase) && (parse.children.Length >= 1))
            {
                bool positive = true;
                int cnt = 1;
                int idx = 0;
                if (parse.children[0].rule.Equals("not", StringComparison.InvariantCultureIgnoreCase) && (parse.children.Length == 2))
                {
                    idx = 1;
                    cnt = 2;
                    positive = false;
                }
                if (parse.children[idx].rule.Equals("item", StringComparison.InvariantCultureIgnoreCase) && (parse.children.Length == cnt))
                {
                    switch (parse.children[idx].children[0].rule.ToLower())
                    {
                        case "text": return new QWord(search, text, parse, !positive);

                        case "wildcard": return new QWildcard(search, text, parse, !positive);

                        case "lemma": return new QLemma(search, text, parse, !positive);

                        case "pos":
                        case "pos32":
                        case "pn_pos12": return new QPartOfSpeech(search, text, parse, !positive);

                        case "loc":
                        case "seg": return new QTransition(search, text, parse, !positive);

                        case "punc": return new QPunctuation(search, text, parse, !positive);

                        case "italics":
                        case "jesus": return new QDecoration(search, text, parse, !positive);

                        case "greek":
                        case "hebrew": return new QStrongs(search, text, parse, !positive);

                        case "delta": return new QDelta(search, text, parse, !positive);
                    }
                }
            }
            return null;
        }
    }
}