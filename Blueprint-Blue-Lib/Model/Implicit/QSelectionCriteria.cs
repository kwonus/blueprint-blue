namespace Blueprint.Blue
{
    using AVSearch.Model.Results;
    using Blueprint.Model.Implicit;
    using Pinshot.PEG;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

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

            if (criteria.rule.Equals("selection_statement", StringComparison.InvariantCultureIgnoreCase) && (criteria.children.Length >= 1))
            {
                Parsed? expression = null;

                foreach (Parsed block in criteria.children)
                {
                    if (block.children.Length > 1)
                    {
                        switch (block.rule.ToLower())
                        {
                            case "settings_block":   foreach (Parsed setting in block.children)
                                                        selection.SettingsFactory(setting, possibleMacro: false);
                                                     user_supplied_settings = true;
                                                     break;  
                            case "scoping_block":    foreach (Parsed filter in block.children)
                                                        selection.FilterFactory(filter, possibleMacro: false);
                                                     break; 
                        }
                    }
                    else if (block.children.Length == 1)
                    {
                        switch (block.rule.ToLower())
                        {
                            case "expression_block": expression = block.children[0]; 
                                                     break;      
                            case "settings_block":   selection.SettingsFactory(block.children[0], possibleMacro: true);
                                                     user_supplied_settings = true;
                                                     break;  
                            case "scoping_block":    selection.FilterFactory(block.children[0], possibleMacro: true);
                                                     break; 
                        }
                    }
                }
                if (user_supplied_settings)
                {
                    foreach (QAssign assignment in selection.Assignments)
                        selection.Settings.Assign(assignment);
                }
                // SearchExpression(QFind) subsumes expression, settings, and filters in selection object
                // (they need not be processed in this method when expression is part of imperative)
                selection.SearchExpression = QFind.Create(env, selection, selection.Text, expression, user_supplied_settings);
            }
            return selection;
        }
        private void FilterFactory(Parsed filter, bool possibleMacro)
        {
            QFilter? instance = QFilter.Create(filter, this.Context);

            if (instance != null)
            {
                this.Scope.Add(instance);
            }
            else if (possibleMacro)
            {
                if (filter.rule.StartsWith("hashtag_") && (filter.children.Length == 1))
                {
                    // this is a partial utilization [macro or history]
                    var invocation = QUtilize.Create(this.Context, filter.text, filter.children[0]);
                    if (invocation != null)
                    {
                        foreach (QFilter item in invocation.Filters)
                        {
                            this.Scope.Add(item);
                        }
                    }
                }
            }
        }

        private void SettingsFactory(Parsed variable, bool possibleMacro)
        {
            if (variable.children.Length == 1)
            {
                QAssign? assignment = QVariable.CreateAssignment(this.Context, variable.text, variable.children[0]);

                if (assignment != null)
                {
                    this.Assignments.Add(assignment);
                }
                else if (possibleMacro)
                {
                    if (variable.rule.StartsWith("hashtag_") && (variable.children.Length == 1))
                    {
                        // this is a partial utilization [macro or history]
                        var invocation = QUtilize.Create(this.Context, variable.text, variable.children[0]);
                        if (invocation != null)
                        {
                            this.Settings.CopyFrom(invocation.Settings);
                            this.UtilizeAssignments = invocation;
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