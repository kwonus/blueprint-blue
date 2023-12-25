namespace Blueprint.Blue
{
    using AVSearch.Model.Features;
    using Pinshot.PEG;
    using System;

    public abstract class FeatureFactory // This is mostly redundant with QSearchFeature. The inheritance aspect of this class needs to be preserved 
    {
        public static FeatureGeneric? Create(QFind search, string text, Parsed parse)
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
                if (parse.children[idx].rule.Equals("item", StringComparison.InvariantCultureIgnoreCase) && (parse.children.Length == cnt) && parse.children[idx].children.Length == 1)
                {
                    var child = parse.children[idx].children[0];

                    switch (parse.children[idx].children[0].rule.ToLower())
                    {
                        case "wildcard":
                        case "text": return new QLexeme(search, text, child, !positive);

                        case "lemma": return new QLemma(search, text, child, !positive);

                        case "pos":
                        case "pos32":
                        case "pn_pos12": return new QPartOfSpeech(search, text, child, !positive);

                        case "loc":
                        case "seg": return new QTransition(search, text, child, !positive);

                        case "punc": return new QPunctuation(search, text, child, !positive);

                        case "italics":
                        case "jesus": return new QDecoration(search, text, child, !positive);

                        case "greek":
                        case "hebrew": return new QStrongs(search, text, child, !positive);

                        case "delta": return new QDelta(search, text, child, !positive);
                    }
                }
            }
            return null;
        }
    }
}