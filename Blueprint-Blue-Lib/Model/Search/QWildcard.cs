namespace Blueprint.Blue
{
    using AVXLib;
    using BlueprintBlue.Model.Implicit;
    using Pinshot.PEG;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class QWildcard : QFeature, IFeature
    {
        override public string Type { get => QFeature.GetTypeName(this); }
        public UInt16[] WordKeys { get; set; }

        public QWildcard(QFind search, string text, Parsed parse, bool negate) : base(search, text, parse, negate)
        {
            string? beginning = null;
            string? ending = null;

            string normalized = text.Remove('-');

            int star = normalized.IndexOf('*');
            if (star >= 0)
            {
                var lexicon = ObjectTable.AVXObjects.Mem.Lexicon.Slice(1).ToArray();

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

                if (search.Settings.Lexicon.Value != QLexicalDomain.QLexiconVal.AVX)
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
                if (search.Settings.Lexicon.Value != QLexicalDomain.QLexiconVal.AV)
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
    }
}