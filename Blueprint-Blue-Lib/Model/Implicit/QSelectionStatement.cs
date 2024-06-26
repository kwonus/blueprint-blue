﻿namespace Blueprint.Blue
{
    using Pinshot.PEG;
    using System;
    using YamlDotNet.Serialization;
    using System.Text.Json.Serialization;
    using Blueprint.Model.Implicit;
    using AVSearch.Model.Results;
    using System.Text.RegularExpressions;
    using AVSearch.Interfaces;
    using static Blueprint.Model.Implicit.QFormat;
    using System.Security.Authentication.ExtendedProtection;
    using System.Text;

    public enum DirectiveResultType
    {
        ExportFailed = -3,
        ExportNotReady = -2,
        MacroCreationFailed = -1,
        NotApplicable = 0,
        MacroCreated = 1,
        ExportReady = 2,
        ExportSuccessful = 3,
    }
    public enum SelectionResultType
    {
        InvalidStatement = -1,
        NoResults = 0,
        SingletonSuccess = 1,
        SearchResults = 2,
        ScopeOnlyResults = 3,
     }

    public class QSelectionStatement
    {
        [JsonIgnore]
        [YamlIgnore]
        public QContext Context { get; set; }

        public QueryResult Results { get; set; }

        public ExportDirective? ExportDirective { get; internal set; }
        public MacroDirective?  MacroDirective  { get; internal set; }

        public QSelectionCriteria SelectionCriteria { get; internal set; }
        public SelectionResultType Status { get; internal set; }

        public (SelectionResultType ok, DirectiveResultType directive, QueryResult query) Execute()
        {
            if (this.SelectionCriteria.SearchExpression != null)
            {
                DirectiveResultType directive = DirectiveResultType.NotApplicable;
                SelectionResultType executed = this.SelectionCriteria.SearchExpression.Expression != null
                    ? this.Search(this.SelectionCriteria.SearchExpression)
                    : this.Show(this.SelectionCriteria.SearchExpression);
                if ((this.ExportDirective != null) && (executed == SelectionResultType.SearchResults || executed == SelectionResultType.ScopeOnlyResults))
                {
                    directive = this.ExportDirective.Retrieve();
                }
                return (executed, directive, this.Results);
            }
            return (SelectionResultType.InvalidStatement, DirectiveResultType.NotApplicable, this.Results);
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
            if (search.Scope.Count > 0 && result == SelectionResultType.NoResults)
            {
                result = SelectionResultType.ScopeOnlyResults;
            }
            this.Status = result;

            return result;
        }
        private SelectionResultType Show(QFind search) // this executes a selection that has no search criteria
        {
            SelectionResultType result = search.Fragments.Count > 0 ? SelectionResultType.NoResults : SelectionResultType.InvalidStatement;

            for (byte b = 1; b <= 66; b++)
            {
                if (search.Scope.InScope(b))
                {
                    QueryBook qbook = new(b);
                    search.Books[b] = qbook;
                    if (qbook.Show(search) && result == SelectionResultType.NoResults)
                        return SelectionResultType.SearchResults;
                }
            }

            return SelectionResultType.NoResults;
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

                if (stmt.children.Length >= 2 && seg != null)
                {
                    Parsed directive = stmt.children[stmt.children.Length-1];
                    switch (directive.rule)
                    {
                        case "macro_directive":  selection.MacroDirective  = MacroDirective.Create(context, directive.text, directive); break;
                        case "export_directive": selection.ExportDirective = ExportDirective.Create(context, directive.text, directive.children, seg); break;
                    }
                }
            }
            selection.Results = new QueryResult(selection.SelectionCriteria.SearchExpression);

            return selection;
        }
    }
}