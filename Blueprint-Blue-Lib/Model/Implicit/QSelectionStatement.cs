namespace Blueprint.Blue
{
    using Pinshot.PEG;
    using System;
    using YamlDotNet.Serialization;
    using System.Text.Json.Serialization;
    using Blueprint.Model.Implicit;
    using AVSearch.Model.Results;
    using System.Text.RegularExpressions;

    public enum SelectionResultType
    {
        InvalidStatement = -1,
        NoResults = 0,
        SearchResults = 1,
        ScopeOnlyResults = 2,
        SingletonSuccess = 3,
    }

    public class QSelectionStatement
    {
        [JsonIgnore]
        [YamlIgnore]
        public QContext Context { get; set; }

        public QueryResult Results { get; set; }

        public QExport? ExportDirective { get; internal set; }
        public QApply?  MacroDirective  { get; internal set; }

        public QSelectionCriteria SelectionCriteria { get; internal set; }
        public SelectionResultType Status { get; internal set; }

        public (SelectionResultType ok, QueryResult query) Execute()
        {
            if (this.SelectionCriteria.SearchExpression != null)
            {
                SelectionResultType executed = this.Search(this.SelectionCriteria.SearchExpression);
                return (executed, this.Results);
            }
            return (SelectionResultType.InvalidStatement, this.Results);
        }
        private SelectionResultType Search(QFind search)
        {
            SelectionResultType result = search.Fragments.Count > 0 ? SelectionResultType.NoResults : SelectionResultType.InvalidStatement;

            if (result == SelectionResultType.NoResults)
            {
                for (byte b = 1; b <= 66; b++)
                {
                    if ((search.Scope.Count == 0) || search.Scope.InScope(b))
                    {
                        QueryBook qbook = new(b);
                        search.Books[b] = qbook;
                        if (qbook.Search(search) && result == SelectionResultType.NoResults)
                            result = SelectionResultType.SearchResults;
                    }
                }
            }
            else if (search.Scope.Count > 0)
            {
                result = SelectionResultType.ScopeOnlyResults;
            }
            this.Status = result;

            return result;
        }

        private QSelectionStatement(QContext env, string stmtText)
        {
            this.Context = env;
            this.ExportDirective = null;
            this.MacroDirective = null;
            this.SelectionCriteria = null;
            this.Status = SelectionResultType.NoResults;
        }

        public static QSelectionStatement? Create(QContext context, Parsed stmt, QStatement diagnostics)
        {
            var selection = new QSelectionStatement(context, stmt.text);

            if (stmt.rule.Equals("selection_statement", StringComparison.InvariantCultureIgnoreCase) && (stmt.children.Length >= 1))
            {
                QSelectionCriteria? seg = QSelectionCriteria.CreateSelectionCriteria(context, selection.Results, stmt);
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