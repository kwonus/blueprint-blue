namespace Blueprint.Blue
{
    using Pinshot.PEG;
    using System.Collections.Generic;
    using System;
    using YamlDotNet.Serialization;
    using System.Text.Json.Serialization;
    using Blueprint.Model.Implicit;
    using static AVXLib.Framework.Numerics;
    using AVSearch.Model.Results;
    using System.Runtime.CompilerServices;
    using AVSearch.Model.Expressions;

    public class QSearchStatement
    {
        [JsonIgnore]
        [YamlIgnore]
        public QContext Context { get; set; }

        public QueryResult Results { get; set; }

        public QExport? ExportDirective { get; internal set; }
        public QApply?  MacroDirective  { get; internal set; }

        public QSelectionCriteria SelectionCriteria { get; internal set; }

        public (bool ok, QueryResult query) Execute()
        {

            if (this.SelectionCriteria.FindExpression != null)
            {
                bool executed = this.SelectionCriteria.Scope.Count == 0
                    ? this.Search(this.SelectionCriteria.FindExpression)
                    : this.SearchWithScope(this.SelectionCriteria.FindExpression);

                var exp = this.SelectionCriteria.FindExpression;
                {
                    for (byte b = 1; b <= 66; b++)
                        if (exp.Books.ContainsKey(b) && (exp.Books[b].Chapters.Count == 0))
                            exp.Books.Remove(b);
                }
                return (executed, this.Results);
            }
            else  // TO DO: account for selection criteria w/o a FindExpression
            {
                return (false, new QueryResult());
            }
        }
        private bool Search(QFind search)
        {
            bool result = search.Fragments.Count > 0;

            if (result)
            {
                search.AddScope(0);
                foreach (var book in search.Books.Values)
                {
                    result = book.Search(search) || result;
                }
            }
            return result;
        }
        private bool SearchWithScope(QFind search)
        {
            bool result = search.Fragments.Count > 0;

            if (result)
            {
                foreach (var filter in search.Scope.Values)
                {
                    search.AddScope(filter);
                }
                foreach (var book in search.Books.Values)
                {
                    result = result || book.Search(search);
                }
            }
            return result;
        }

        private QSearchStatement(QContext env, string stmtText)
        {
            this.Context = env;
            this.ExportDirective = null;
            this.MacroDirective = null;
            this.SelectionCriteria = null;
        }

        public static QSearchStatement? Create(QContext context, Parsed stmt, QStatement diagnostics)
        {
            bool valid = false;
            var selection = new QSearchStatement(context, stmt.text);

            if (stmt.rule.Equals("implicits", StringComparison.InvariantCultureIgnoreCase) && (stmt.children.Length >= 1))
            {
                Parsed[] segments = stmt.children;

                for (int s = 0; s < segments.Length; s++)
                {
                    Parsed segment = segments[s];

                    if ((segment.children.Length >= 1) && segment.children[0].rule.StartsWith("elements", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var elements = segment.children[0];
                        if ((segment.children.Length == 2) && segment.children[1].rule.StartsWith("apply_macro_", StringComparison.InvariantCultureIgnoreCase))
                        {
                            var macro = segment.children[1];
                            selection.MacroDirective = QApply.Create(context, segment.text, macro);
                        }
                        QSelectionCriteria seg = QSelectionCriteria.CreateSegment(context, selection.Results, elements, selection.MacroDirective);
                        valid = (seg != null);
                        if (seg != null)
                            selection.SelectionCriteria = seg;
                    }
                }
            }
            selection.Results = new QueryResult(selection.SelectionCriteria.FindExpression);

            return valid ? selection : null;
        }
    }
}