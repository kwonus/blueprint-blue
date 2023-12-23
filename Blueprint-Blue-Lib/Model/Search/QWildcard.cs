namespace Blueprint.Blue
{
    using AVXLib;
    using BlueprintBlue.Model.Implicit;
    using Pinshot.PEG;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection.Metadata.Ecma335;
    using System.Text;

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
            if (parse.rule.Equals("term_begin", StringComparison.InvariantCultureIgnoreCase))
            {
                if (parse.text.Contains('-'))
                {
                    this.BeginningHyphenated = this.Text;
                    this.Beginning = this.Text.Replace("-", "");
                }
                else
                {
                    this.Beginning = this.Text;
                }
            }
            else if (parse.rule.Equals("term_end", StringComparison.InvariantCultureIgnoreCase))
            {
                if (parse.text.Contains('-'))
                {
                    this.EndingHyphenated = this.Text;
                    this.Ending = this.Text.Replace("-", "");
                }
                else
                {
                    this.Ending = this.Text;
                }
            }
            else if (parse.rule.Equals("term_contains", StringComparison.InvariantCultureIgnoreCase))
            {
                if (parse.text.Contains('-'))
                {
                    var normalized = this.Text.Replace("-", "");

                    this.Contains.Add(normalized);
                    this.ContainsHyphenated.Add(this.Text);
                }
                else
                {
                    this.Contains.Add(this.Text);
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
                    if (child.rule.Equals("term_begin_and_end", StringComparison.InvariantCultureIgnoreCase) && (child.children != null))
                    {
                        foreach (var grandchild in child.children)
                        {
                            this.AddParts(grandchild);
                        }
                    }
                    else
                    {
                        this.AddParts(child);
                    }
                }
            }
        }
        public UInt16[] GetLexemes(QSettings settings)
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

                kjvMatch.normalized = (settings.Lexicon.Value == QLexicalDomain.QLexiconVal.AV || settings.Lexicon.Value == QLexicalDomain.QLexiconVal.BOTH)
                    && ((this.Beginning == null) || kjvNorm.StartsWith(this.Beginning, StringComparison.InvariantCultureIgnoreCase))
                    && ((this.Ending    == null) || kjvNorm.EndsWith(  this.Ending,    StringComparison.InvariantCultureIgnoreCase));

                avxMatch.normalized = (settings.Lexicon.Value == QLexicalDomain.QLexiconVal.AVX || settings.Lexicon.Value == QLexicalDomain.QLexiconVal.BOTH)
                    && ((this.Beginning == null) || avxNorm.StartsWith(this.Beginning, StringComparison.InvariantCultureIgnoreCase))
                    && ((this.Ending    == null) || avxNorm.EndsWith(  this.Ending,    StringComparison.InvariantCultureIgnoreCase));

                match = kjvMatch.normalized || avxMatch.normalized;

                if (hyphenated)
                {
                    kjvMatch.hyphenated = (settings.Lexicon.Value == QLexicalDomain.QLexiconVal.AV || settings.Lexicon.Value == QLexicalDomain.QLexiconVal.BOTH)
                        && ((this.Beginning == null) || kjv.StartsWith(this.Beginning, StringComparison.InvariantCultureIgnoreCase))
                        && ((this.Ending    == null) || kjv.EndsWith(  this.Ending,    StringComparison.InvariantCultureIgnoreCase));

                    avxMatch.hyphenated = (settings.Lexicon.Value == QLexicalDomain.QLexiconVal.AVX || settings.Lexicon.Value == QLexicalDomain.QLexiconVal.BOTH)
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