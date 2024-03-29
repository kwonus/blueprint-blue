namespace Blueprint.Blue
{
    using Pinshot.PEG;
    using System;
    using System.Collections.Generic;
    using XBlueprintBlue;
    public interface IFeature
    {
        string Text { get; }
        IEnumerable<string> AsYaml();
    }

    public abstract class QFeature : IFeature // This is mostly redundant with QSearchFeature. The inheritance aspect of this class needs to be preserved 
    {
        public string Text { get; private set; }
        public Parsed Parse { get; private set; }
        public QFind Search { get; private set; }
        protected QFeature(QFind context, string text, Parsed parse)
        {
            this.Text = text;
            this.Parse = parse;
            this.Search = context;
        }
        public static QFeature? Create(QFind search, string text, Parsed parse)
        {
            if (parse.rule.Equals("feature", StringComparison.InvariantCultureIgnoreCase) && (parse.children.Length == 1))
            {
                switch (parse.children[0].rule.ToLower())
                {
                    case "text":     return new QWord(search, text, parse);

                    case "wildcard": return new QWildcard(search, text, parse);

                    case "lemma":    return new QLemma(search, text, parse);

                    case "pos":      
                    case "pos32":    
                    case "pn_pos12": return new QPartOfSpeech(search, text, parse);

                    case "loc":
                    case "seg":      return new QTransition(search, text, parse);

                    case "punc":     return new QPunctuation(search, text, parse);

                    case "italics": 
                    case "jesus":    return new QDecoration(search, text, parse);

                    case "greek":
                    case "hebrew":   return new QStrongs(search, text, parse);
                }
            }
            return null;
        }
        public abstract IEnumerable<string> AsYaml();

        public abstract XFeature AsMessage();
    }
}