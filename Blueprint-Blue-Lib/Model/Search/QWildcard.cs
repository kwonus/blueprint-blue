namespace Blueprint.Blue
{
    using AVSearch.Interfaces;
    using AVSearch.Model.Types;
    using AVXLib;
    using Blueprint.Model.Implicit;
    using Pinshot.PEG;
    using System;
    using System.Collections.Generic;

    public class QWildcard: TypeWildcard
    {
        private void AddParts(Parsed parse)
        {
            if ((parse.children == null) || (parse.children.Length != 1))
                return;

            if (parse.children[0].rule.Equals("nuphone", StringComparison.InvariantCultureIgnoreCase))
                this.TermType = WildcardType.NuphoneTerm;
            else if (parse.children[0].rule.Equals("text", StringComparison.InvariantCultureIgnoreCase))
                this.TermType = WildcardType.EnglishTerm;
            else
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

        public QWildcard(string text, Parsed parse, WildcardType type) : base(text, type)
        {
            this.Text = text.Trim();

            this.Beginning = null;
            this.Ending = null;

            this.BeginningHyphenated = null;
            this.EndingHyphenated = null;

            if (parse.children != null)
            {
                foreach (var child in parse.children)
                {
                    this.AddParts(child);
                }
            }
        }
    }
}