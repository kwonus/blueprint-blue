namespace Blueprint.Blue
{
    using AVSearch.Model.Expressions;
    using Pinshot.PEG;
    using System.Collections.Generic;

    public class QFind: SearchExpression, IDiagnostic
    {
        private IDiagnostic Diagnostics;

        public void AddFilters(IEnumerable<QFilter> filters)
        {
            foreach (var filter in filters)
            {
                var results = ScopingFilter.Create(filter.Filter);
                if (results != null)
                {
                    foreach (ScopingFilter result in results)
                    {
                        if (!this.Scope.ContainsKey(result.Book))
                            this.Scope[result.Book] = result;
                        else
                            this.Scope[result.Book].Ammend(result);
                    }
                }  
            }
        }
        private QFind(IDiagnostic diagnostics, QSelectionCriteria selection, string text, Parsed? expression, bool useExplicitSettings): base(selection.Settings, selection.Results)
        {
            this.IsValid = false;
            this.Diagnostics = diagnostics;
            this.Scope = new();
            this.Expression = text;
            this.Settings = selection.Settings;
            this.AddFilters(selection.Scope);

            this.Fragments = new();
            bool validExpression = (expression != null) && (expression.rule == "search" && expression.children.Length == 1) && !string.IsNullOrWhiteSpace(text);
            if ((expression != null) && validExpression)
            {
                Parsed child = expression.children[0];

                bool ordered = child.rule.Equals("ordered") && (child.children.Length > 0);
                bool unordered = child.rule.Equals("unordered") && (child.children.Length > 0);

                if (ordered || unordered)
                {
                    this.Expression = string.Empty;

                    this.Quoted = ordered;

                    string fulltext = text.Trim();

                    foreach (Parsed gchild in child.children)
                    {
                        bool anchored = gchild.rule.Equals("fragment") && (gchild.children.Length > 0);
                        bool unanchored = gchild.rule.Equals("unanchored") && (gchild.children.Length == 1)
                            && gchild.children[0].rule.Equals("fragment") && (gchild.children[0].children.Length > 0);

                        validExpression = anchored || unanchored;

                        if (validExpression)
                        {
                            Parsed frag = anchored ? gchild : gchild.children[0];
                            QFragment fragment = new QFragment(this, frag, anchored);
                            this.Fragments.Add(fragment);
                        }
                        else break;
                    }
                }
                if (validExpression)
                {
                    this.IsValid = true;
                    return;
                }
            }
            validExpression = (expression != null) && (expression.rule.StartsWith("hashtag_") && expression.children.Length == 1) && !string.IsNullOrWhiteSpace(text);
            if (validExpression && (expression != null))
            {
                var invocation = QUtilize.Create(selection.Context, expression.text, expression.children[0]);
                if (invocation != null)
                {
                    selection.SearchExpression = invocation.Expression;
                    if (selection.Scope.Count == 0)
                        selection.Scope = invocation.Filters;
                    if (!useExplicitSettings)
                        selection.Settings.CopyFrom(invocation.Settings);
                    this.IsValid = true;
                    return;
                }
                validExpression = false;
            }
            if (useExplicitSettings)
            {
                this.IsValid = true;
            }
            this.Expression = string.Empty; // fall through here, makes *expression* invalid (even though selection might be valid)

            this.IsValid = this.IsValid || (this.Scope.Count > 0);
        }
        public static QFind? Create(IDiagnostic diagnostics, QSelectionCriteria selection, string text, Parsed? expression, bool useExplicitSettings)
        {
            // TODO: Consider that this could be a full or demoted/partial macro and process accordingly
            //
            QFind? search = new QFind(diagnostics, selection, text, expression, useExplicitSettings);
            return search.IsValid ? search : null;
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