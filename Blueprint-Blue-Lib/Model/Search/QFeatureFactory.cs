namespace Blueprint.Blue
{
    using AVSearch.Interfaces;
    using AVSearch.Model.Features;
    using Pinshot.PEG;
    using System;

    public abstract class FeatureFactory // This is mostly redundant with QSearchFeature. The inheritance aspect of this class needs to be preserved 
    {
        public static FeatureGeneric? Create(QFind search, string text, Parsed parse)
        {
            if (parse.rule.Equals("feature_option", StringComparison.InvariantCultureIgnoreCase) && (parse.children.Length >= 1))
            {
                foreach (Parsed feature in parse.children)
                {
                    if (feature.rule.Equals("feature", StringComparison.InvariantCultureIgnoreCase) && (parse.children.Length >= 1))
                    {
                        bool positive = true;

                        Parsed item = feature.children[0];
                        if (feature.rule.Equals("not", StringComparison.InvariantCultureIgnoreCase) && (feature.children.Length == 2))
                        {
                            item = feature.children[1];
                            positive = false;
                        }
                        if (item.rule.Equals("item", StringComparison.InvariantCultureIgnoreCase) && (item.children.Length == 1) && parse.children[0].children.Length == 1)
                        {
                            Parsed type = item.children[0];
                            switch (type.rule.ToLower())
                            {
                                case "wildcard":
                                case "text":     return new QLexeme(search, text, type, !positive);

                                case "lemma":    return new QLemma(search, text, type, !positive);

                                case "pos":
                                case "pos32":
                                case "pn_pos12": return new QPartOfSpeech(search, text, type, !positive);

                                case "loc":
                                case "seg":      return new QTransition(search, text, type, !positive);

                                case "punc":     return new QPunctuation(search, text, type, !positive);

                                case "italics":
                                case "jesus":    return new QDecoration(search, text, type, !positive);

                                case "greek":
                                case "hebrew":   return new QStrongs(search, text, type, !positive);

                                case "delta":    return new QDelta(search, text, type, !positive);

                                case "entities": return new QEntity(search, text, type, !positive);
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}