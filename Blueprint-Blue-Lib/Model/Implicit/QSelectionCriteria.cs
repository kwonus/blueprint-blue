namespace Blueprint.Blue
{
    using AVSearch.Model.Results;
    using Blueprint.Model.Implicit;
    using Pinshot.PEG;
    using System;
    using System.Collections.Generic;

    public class QSelectionCriteria : QCommand, ICommand
    {
        public QFind?        SearchExpression   { get; protected internal set; }
        public QUtilize?     UtilizeExpression  { get; protected internal set; }
        public List<QFilter> Scope              { get; protected internal set; }
        public QUtilize?     UtilizeScope       { get; protected internal set; }
        public List<QAssign> Assignments        { get; protected internal set; }
        public QUtilize?     UtilizeAssignments { get; protected internal set; }
        public QSettings     Settings           { get; protected internal set; }
        public QueryResult   Results            { get; protected internal set; }

        private QSelectionCriteria(QContext env, QueryResult results, string text, string verb) : base(env, text, verb)
        {
            this.SearchExpression = null;
            this.Assignments = new();
            this.Scope = new();
            this.Settings = new QSettings(env.GlobalSettings);
            this.Results = results;

            this.UtilizeExpression = null;
            this.UtilizeScope = null;
            this.UtilizeAssignments = null;
    }
        public static QSelectionCriteria CreateSelectionCriteria(QContext env, QueryResult results, Parsed criteria)
        {
            var selection = new QSelectionCriteria(env, results, criteria.text, criteria.rule);
            bool user_supplied_settings = false;

            if (criteria.rule.Equals("selection_criteria", StringComparison.InvariantCultureIgnoreCase) && (criteria.children.Length >= 1))
            {
                Parsed? expression = null;

                foreach (Parsed block in criteria.children)
                {
                    if (block.children.Length > 1)
                    {
                        switch (block.rule.ToLower())
                        {
                            case "settings_block":   foreach (Parsed setting in block.children)
                                                        QSelectionCriteria.SettingsFactory(selection, setting, possibleMacro: false);
                                                     user_supplied_settings = true;
                                                     break;  
                            case "scoping_block":    foreach (Parsed filter in block.children)
                                                        QSelectionCriteria.FilterFactory(selection, filter, possibleMacro: false);
                                                     break; 
                        }
                    }
                    else if (block.children.Length == 1)
                    {
                        switch (block.rule.ToLower())
                        {
                            case "expression_block": expression = block.children[0]; 
                                                     break;      
                            case "settings_block":   QSelectionCriteria.SettingsFactory(selection, block.children[0], possibleMacro: true);
                                                     user_supplied_settings = true;
                                                     break;  
                            case "scoping_block":    QSelectionCriteria.FilterFactory(selection, block.children[0], possibleMacro: true);
                                                     break; 
                        }
                    }
                }
                // SearchExpression(QFind) subsumes expression, settings, and filters in selection object
                // (they need not be processed in this method when expression is part of imperative)
                selection.SearchExpression = QFind.Create(env, selection, selection.Text, expression, user_supplied_settings);
            }
            return selection;
        }
        private static void FilterFactory(QSelectionCriteria selection, Parsed filter, bool possibleMacro)
        {
            QFilter? instance = QFilter.Create(filter);

            if (instance != null)
            {
                selection.Scope.Add(instance);
            }
            else if (possibleMacro)
            {
                if (filter.rule.StartsWith("hashtag"))
                {
                    // this is a partial utilization [macro or history]
                    var invocation = QUtilize.Create(selection.Context, filter.text, filter.children);
                    if (invocation != null)
                    {
                        foreach (QFilter item in invocation.Filters)
                        {
                            selection.Scope.Add(item);
                        }
                    }
                }
            }
        }

        private static void SettingsFactory(QSelectionCriteria selection, Parsed variable, bool possibleMacro)
        {
            if (variable.children.Length == 1)
            {
                QAssign? assignment = QVariable.CreateAssignment(selection.Context, variable.text, variable.children[0]);

                if (assignment != null)
                {
                    selection.Assignments.Add(assignment);
                }
                else if (possibleMacro)
                {
                    if (variable.rule.StartsWith("hashtag"))
                    {
                        // this is a partial utilization [macro or history]
                        var invocation = QUtilize.Create(selection.Context, variable.text, variable.children);
                        if (invocation != null)
                        {
                            selection.Settings.CopyFrom(invocation.Settings);
                            selection.UtilizeAssignments = invocation;
                        }
                    }
                }
            }
        }

        internal void ConditionallyUpdateSpanToFragmentCount()
        {
            if (this.SearchExpression != null && this.SearchExpression.Settings.SearchSpan != 0)
            {
                UInt16 fragCnt = (UInt16)this.SearchExpression.Fragments.Count;
                if (fragCnt > this.SearchExpression.Settings.SearchSpan)
                {
                    this.Settings.Span.Update(fragCnt);
                }
            }
        }
    }
}