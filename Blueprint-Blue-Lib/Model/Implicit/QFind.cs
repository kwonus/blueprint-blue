namespace Blueprint.Blue
{
    using AVSearch.Model.Expressions;
    using Pinshot.PEG;
    using System;
    using System.Collections.Generic;
    using YamlDotNet.Serialization;
    using static AVXLib.Framework.Numerics;

    public class QFind: SearchExpression, IDiagnostic
    {
        private IDiagnostic Diagnostics;

        public void AddFilters(IEnumerable<QFilter> filters)
        {
            foreach (var filter in filters)
            {
                if (!this.Scope.ContainsKey(filter.Filter))
                    this.Scope[filter.Filter] = filter;
            }
        }
        public static void AddFilters(Dictionary<string, SearchFilter> baseline, IEnumerable<QFilter> filters)
        {
            foreach (var filter in filters)
            {
                if (!baseline.ContainsKey(filter.Filter))
                    baseline[filter.Filter] = filter;
            }
        }
        public static void AddFilter(Dictionary<string, SearchFilter> baseline, QFilter filter)
        {
            if (!baseline.ContainsKey(filter.Filter))
                baseline[filter.Filter] = filter;
        }

        private QFind(IDiagnostic diagnostics, QSelectionCriteria segment, Dictionary<string, SearchFilter> filters, string text, Parsed[] args): base(segment.Settings, segment.Results)
        {
            this.Diagnostics = diagnostics;
            this.Scope = filters;
            this.Expression = text;

            this.Fragments = new();
            this.Valid = (args.Length == 1 && args[0].rule == "search" && args[0].children.Length == 1);
            if (this.Valid)
            {
                Parsed child = args[0].children[0];

                bool ordered = child.rule.Equals("ordered") && (child.children.Length > 0);
                bool unordered = child.rule.Equals("unordered") && (child.children.Length > 0);

                this.Valid = ordered || unordered;
                this.Quoted = ordered;

                string fulltext = text.Trim();
                var beginQuote = fulltext.StartsWith("\"");
                var endQuote = fulltext.StartsWith("\"");

                if (this.Valid)
                {
                    foreach (Parsed gchild in child.children)
                    {
                        bool anchored = gchild.rule.Equals("fragment") && (gchild.children.Length > 0);
                        bool unanchored = gchild.rule.Equals("unanchored") && (gchild.children.Length == 1)
                            && gchild.children[0].rule.Equals("fragment") && (gchild.children[0].children.Length > 0);

                        this.Valid = anchored || unanchored;

                        if (this.Valid)
                        {
                            Parsed frag = anchored ? gchild : gchild.children[0];
                            QFragment fragment = new QFragment(this, frag, anchored);
                            this.Fragments.Add(fragment);
                        }
                        else break;
                    }
                }
            }
        }
        public static QFind? Create(IDiagnostic diagnostics, QSelectionCriteria segment, Dictionary<string, SearchFilter> filters, string text, Parsed[] args)
        {
            QFind? search = new QFind(diagnostics, segment, filters, text, args);

            return search.Valid ? search : null;
        }
        public List<string> AsYaml()
        {
            return ICommand.YamlSerializer(this);
        }
        public void AddError(string message)
        {
            this.Diagnostics.AddError(message);
        }
        public void AddWarning(string message)
        {
            this.Diagnostics.AddWarning(message);
        }
    }
}