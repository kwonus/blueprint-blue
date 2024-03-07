namespace Blueprint.Blue
{
    using AVSearch.Model.Expressions;
    using AVSearch.Model.Results;
    using Blueprint.Model.Implicit;
    using Pinshot.PEG;
    using System;
    using System.Collections.Generic;
    using static System.Reflection.Metadata.BlobBuilder;

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
            var segment = new QSelectionCriteria(env, results, criteria.text, criteria.rule);

            if (criteria.rule.Equals("selection_criteria", StringComparison.InvariantCultureIgnoreCase) && (criteria.children.Length >= 1))
            {
                Dictionary<string, Parsed[]?> blocks = new() {
                    { "expression_block", null },
                    { "settings_block", null },
                    { "filer_block", null }
                };

                foreach (Parsed block in criteria.children)
                {
                    if (block.children.Length >= 1)
                    {
                        blocks[block.rule] = block.children;
                    }
                }
                // Expression block will subsume settings and filters 
                // (they need not be processed in this method when expression is part of imperative)
                if (blocks["expression_block"] != null)
                {
                    Parsed[]? expressions = blocks["expression_block"];
                    if (expressions.Length == 1)
                    {
                        Parsed expression = expressions[0];
                        segment.SearchExpression = QFind.Create(env, segment, expression.text, expression, blocks["filter_block"], blocks["settings_block"]);
                    }
                    return segment; // done with Selection/Search Criteria
                }
                if (blocks["settings_block"] != null)
                {
                    // TO DO: add settings
                }
                if (blocks["filters_block"] != null)
                {
                    // TO DO: add filters
                }
            }
            return segment;
        }/*
            // TO DO: Add Scope
            foreach (Parsed clause in selection.children)
                {
                if (clause.rule.Equals("element"))
                {
                    if (clause.children.Length == 1)
                    {
                        var variable = clause.children[0];

                        if (variable.rule.Equals("assignment", StringComparison.InvariantCultureIgnoreCase))
                        {
                            QAssign? assignment = QVariable.CreateAssignment(env, variable.text, variable.children);
                            if (assignment != null)
                            {
                                segment.Assignments.Add(assignment);
                                segment.Settings.Assign(assignment);
                            }
                        }
                        else if (variable.rule.Equals("utilization_partial", StringComparison.InvariantCultureIgnoreCase))
                        {
                            var invocation = QUtilize.Create(env, clause.text, clause.children);
                            if (invocation != null)
                            {
                                segment.Settings.CopyFrom(invocation.Settings);
                                QFind.AddFilters(filters, invocation.Filters);

                                segment.UtilizeAssignments = invocation;
                            }
                        }
                        else if (variable.rule.Equals("filter", StringComparison.InvariantCultureIgnoreCase))
                        {
                            QFilter? filter = QFilter.Create(env, variable.text, clause.children);
                            if (filter != null)
                            {
                                QFind.AddFilter(filters, filter);
                            }
                        }
                    }
                }
            }

            foreach (Parsed clause in selection.children)
            {
                if (clause.rule.Equals("expression"))
                {
                    if (clause.children.Length == 1)
                    {
                        var expression = clause.children[0];

                        if (expression.rule.Equals("search", StringComparison.InvariantCultureIgnoreCase))
                        {
                            segment.SearchExpression = QFind.Create(env, segment, filters, expression.text, clause.children);
                        }
                        else if (expression.rule.Equals("utilization_full", StringComparison.InvariantCultureIgnoreCase))
                        {
                            var invocation = QUtilize.Create(env, clause.text, clause.children);
                            if (invocation != null)
                            {
                                segment.SearchExpression = invocation.Expression;
                                segment.Settings.CopyFrom(invocation.Settings);
                                QFind.AddFilters(filters, invocation.Filters);

                                segment.UtilizeExpression = invocation;
                            }
                        }
                    }
                }
            }
            segment.ConditionallyUpdateSpanToFragmentCount();

            return segment;
        }*/
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