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
    using System.Xml.Linq;
    using YamlDotNet.Core;

    public class QSelectionStatement
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

            if (this.SelectionCriteria.SearchExpression != null)
            {
                bool executed = this.SelectionCriteria.Scope.Count == 0
                    ? this.Search(this.SelectionCriteria.SearchExpression)
                    : this.SearchWithScope(this.SelectionCriteria.SearchExpression);

                var exp = this.SelectionCriteria.SearchExpression;
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

        private QSelectionStatement(QContext env, string stmtText)
        {
            this.Context = env;
            this.ExportDirective = null;
            this.MacroDirective = null;
            this.SelectionCriteria = null;
        }

        public static QSelectionStatement? Create(QContext context, Parsed stmt, QStatement diagnostics)
        {
            var selection = new QSelectionStatement(context, stmt.text);

            if (stmt.rule.Equals("selection_statement", StringComparison.InvariantCultureIgnoreCase) && (stmt.children.Length >= 1))
            {
                Parsed criteria = stmt.children[0];

                QSelectionCriteria? seg = QSelectionCriteria.CreateSelectionCriteria(context, selection.Results, criteria);
                if (seg == null)
                    return null;

                selection.SelectionCriteria = seg;

                if (stmt.children.Length >= 2)
                {
                    Parsed directive = stmt.children[1];
                    switch (directive.rule)
                    {
                        case "macro_directive":  selection.MacroDirective  = QApply.Create(context, directive.text, directive); break;
                        case "export_directive": selection.ExportDirective = QExport.Create(context, directive.text, directive.children); break;
                    }
                }
            }
            selection.Results = new QueryResult(selection.SelectionCriteria.SearchExpression);

            return selection;
        }
    }
}