namespace Blueprint.Blue
{
    using Pinshot.PEG;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using XBlueprintBlue;

    public class QWildcard : QFeature, IFeature
    {
        public UInt16[] WordKeys { get; set; }

        public QWildcard(QFind search, string text, Parsed parse, bool negate) : base(search, text, parse, negate)
        {
            string? beginning = null;
            string? ending = null;

            string normalized = text.Remove('-');

            int star = normalized.IndexOf('*');
            if (star >= 0)
            {
                var lexicon = QContext.AVXObjects.Mem.Lexicon.Slice(1).ToArray();

                HashSet<UInt16> words = new();
                if (star == 0)
                {
                    ending = normalized.Substring(1);
                }
                else if (star == normalized.Length - 1)
                {
                    beginning = normalized.Substring(0, star);
                }
                else
                {
                    beginning = normalized.Substring(0, star);
                    ending = normalized.Substring(star + 1);
                }

                if (this.Search.Context.LocalSettings.Lexicon.Value != QLexicalDomain.QLexiconVal.AVX)
                {
                    UInt16 key = 0;
                    foreach (var lex in lexicon)
                    {
                        ++key;

                        var find = lex.Search.ToString();
                        if (((beginning == null) || find.ToString().StartsWith(beginning, StringComparison.InvariantCultureIgnoreCase))
                        && ((ending == null) || find.ToString().EndsWith(ending, StringComparison.InvariantCultureIgnoreCase)))
                            words.Add(key);
                    }
                }
                if (this.Search.Context.LocalSettings.Lexicon.Value != QLexicalDomain.QLexiconVal.AV)
                {
                    UInt16 key = 0;
                    foreach (var lex in lexicon)
                    {
                        ++key;

                        if (!words.Contains(key))
                        {
                            var find = lex.Modern.ToString().Remove('-');
                            if ( ((beginning == null) || find.ToString().StartsWith(beginning, StringComparison.InvariantCultureIgnoreCase))
                            &&   ((   ending == null) || find.ToString().EndsWith(     ending, StringComparison.InvariantCultureIgnoreCase)) )
                                words.Add(key);
                        }
                    }
                }
                this.WordKeys = words.ToArray();
            }
            else
            {
                search.Context.AddWarning("A wildcard was supplied, but there was no * found in the text");
                this.WordKeys = new UInt16[0];
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
            var keys = new List<UInt16>(this.WordKeys);
            var word = new XWord() { Wkeys = keys };
            var compare = new XCompare(word);
            var feature = new XFeature { Feature = this.Text, Negate = false, Rule = "wildcard", Match = compare };

            return feature;
        }
    }
}