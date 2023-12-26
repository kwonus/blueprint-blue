namespace Blueprint.Blue
{
    using AVSearch.Interfaces;
    using AVXLib;
    using Blueprint.Model.Implicit;
    using Pinshot.PEG;
    using System;
    using System.Collections.Generic;

    public class QWildcard
    {
        public List<string> Contains { get; private set; }
        public string? Beginning { get; private set; }
        public string? Ending { get; private set; }
        public List<string> ContainsHyphenated { get; private set; }
        public string? BeginningHyphenated { get; private set; }
        public string? EndingHyphenated { get; private set; }
        public string Text { get; private set; }

        private void AddParts(Parsed parse)
        {
            if ((parse.children == null) || (parse.children.Length != 1) || !parse.children[0].rule.Equals("text", StringComparison.InvariantCultureIgnoreCase))
                return;

            var text = parse.children[0].text;

            if (parse.rule.Equals("term_begin", StringComparison.InvariantCultureIgnoreCase))
            {
                if (parse.text.Contains('-'))
                {
                    this.BeginningHyphenated = text;
                    this.Beginning = this.Text.Replace("-", "");
                }
                else
                {
                    this.Beginning = text;
                }
            }
            else if (parse.rule.Equals("term_end", StringComparison.InvariantCultureIgnoreCase))
            {
                if (parse.text.Contains('-'))
                {
                    this.EndingHyphenated = text;
                    this.Ending = text.Replace("-", "");
                }
                else
                {
                    this.Ending = text;
                }
            }
            else if (parse.rule.Equals("term_contains", StringComparison.InvariantCultureIgnoreCase))
            {
                if (parse.text.Contains('-'))
                {
                    var normalized = text.Replace("-", "");

                    this.Contains.Add(normalized);
                    this.ContainsHyphenated.Add(text);
                }
                else
                {
                    this.Contains.Add(text);
                }
            }
        }

        public QWildcard(string text, Parsed parse)
        {
            this.Text = text.Trim();

            this.Beginning = null;
            this.Ending = null;
            this.Contains = new();

            this.BeginningHyphenated = null;
            this.EndingHyphenated = null;
            this.ContainsHyphenated = new();

            if (parse.children != null)
            {
                foreach (var child in parse.children)
                {
                    this.AddParts(child);
                }
            }
        }
        public UInt16[] GetLexemes(ISettings settings)
        {
            var lexicon = ObjectTable.AVXObjects.Mem.Lexicon.Slice(1).ToArray();
            var lexemes = new List<UInt16>();

            UInt16 key = 0;
            foreach (var lex in lexicon)
            {
                bool match = false;
                ++key;

                string kjv = lex.Display.ToString();
                string avx = lex.Modern.ToString();

                (bool normalized, bool hyphenated) kjvMatch = (false, false);
                (bool normalized, bool hyphenated) avxMatch = (false, false);

                bool hyphenated = kjv.Contains('-');

                string kjvNorm = hyphenated ? lex.Search.ToString() : kjv;
                string avxNorm = hyphenated ? lex.Search.ToString() : avx;  // transliterated names do not differ between kjv and avx

                kjvMatch.normalized = settings.UseLexiconAV
                    && ((this.Beginning == null) || kjvNorm.StartsWith(this.Beginning, StringComparison.InvariantCultureIgnoreCase))
                    && ((this.Ending    == null) || kjvNorm.EndsWith(  this.Ending,    StringComparison.InvariantCultureIgnoreCase));

                avxMatch.normalized = settings.UseLexiconAVX
                    && ((this.Beginning == null) || avxNorm.StartsWith(this.Beginning, StringComparison.InvariantCultureIgnoreCase))
                    && ((this.Ending    == null) || avxNorm.EndsWith(  this.Ending,    StringComparison.InvariantCultureIgnoreCase));

                match = kjvMatch.normalized || avxMatch.normalized;

                if (hyphenated)
                {
                    kjvMatch.hyphenated = settings.UseLexiconAV
                        && ((this.Beginning == null) || kjv.StartsWith(this.Beginning, StringComparison.InvariantCultureIgnoreCase))
                        && ((this.Ending    == null) || kjv.EndsWith(  this.Ending,    StringComparison.InvariantCultureIgnoreCase));

                    avxMatch.hyphenated = settings.UseLexiconAVX
                        && ((this.Beginning == null) || avx.StartsWith(this.Beginning, StringComparison.InvariantCultureIgnoreCase))
                        && ((this.Ending    == null) || avx.EndsWith(  this.Ending,    StringComparison.InvariantCultureIgnoreCase));

                    match = match || kjvMatch.hyphenated || avxMatch.hyphenated;
                }
                if (match && this.Contains.Count > 0)
                {
                    foreach (var piece in this.Contains)
                    {
                        if (kjvMatch.normalized)
                            kjvMatch.normalized = kjvNorm.Contains(piece, StringComparison.InvariantCultureIgnoreCase);
                        if (avxMatch.normalized)
                            avxMatch.normalized = avxNorm.Contains(piece, StringComparison.InvariantCultureIgnoreCase);

                        if (kjvMatch.hyphenated)
                            kjvMatch.hyphenated = kjv.Contains(piece, StringComparison.InvariantCultureIgnoreCase);
                        if (avxMatch.hyphenated)
                            avxMatch.hyphenated = avx.Contains(piece, StringComparison.InvariantCultureIgnoreCase);
                    }
                }
                if (kjvMatch.normalized || avxMatch.normalized || kjvMatch.hyphenated || avxMatch.hyphenated)
                {
                    lexemes.Add(key);
                }
            }
            return lexemes.ToArray();
        }
    }
}