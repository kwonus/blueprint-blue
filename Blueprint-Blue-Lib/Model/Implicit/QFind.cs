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

        private QFind(IDiagnostic diagnostics, QCommandSegment segment, Dictionary<string, SearchFilter> filters, string text, Parsed[] args): base(segment.Settings, segment.Results)
        {
            this.Diagnostics = diagnostics;
            this.Scope = filters;
            this.Expression = text;

            this.Fragments = new();
            this.Valid = (args.Length > 0 && args[0].children.Length > 0);
            if (this.Valid)
            {
                string fulltext = text.Trim();
                var beginQuote = fulltext.StartsWith("\"");
                var endQuote = fulltext.StartsWith("\"");

                this.Quoted = beginQuote && endQuote;

                this.Valid = this.Quoted ? true : !fulltext.Contains('"');
            }

            if (this.Valid)
            {
                string rule = this.Quoted ? "ordered" : "unordered";
                this.Valid = (args.Length == 1) && (args[0].children.Length == 1) && args[0].children[0].rule.Equals(rule, StringComparison.InvariantCultureIgnoreCase) && (args[0].children[0].children.Length > 0);
            }
            if (this.Valid)
            {
                var child = args[0].children[0];

                foreach (var gchild in child.children)
                {
                    QFragment frag;

                    this.Valid = gchild.rule.Equals("fragment") && (gchild.children.Length > 0);
                    if (this.Valid)
                    {
                        frag = new QFragment(this, gchild.text, gchild.children, anchored: this.Quoted);
                    }
                    else
                    {
                        this.Valid = (gchild.children.Length > 0);
                        if (!this.Valid)
                            break;
                        frag = new QFragment(this, gchild.children[0].text, gchild.children[0].children, anchored: gchild.rule.Equals("anchored"));
                    }
                    if (this.Valid)
                        this.Fragments.Add(frag);
                    else
                        break;
                }
            }

        }
        public static QFind? Create(IDiagnostic diagnostics, QCommandSegment segment, Dictionary<string, SearchFilter> filters, string text, Parsed[] args)
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