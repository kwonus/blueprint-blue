namespace Blueprint.Blue
{
    using AVSearch.Model.Expressions;
    using AVSearch.Model.Results;
    using Blueprint.Model.Implicit;
    using Pinshot.PEG;
    using System;
    using System.Collections.Generic;

    public class QSelectionCriteria : QCommand, ICommand
    {
        public QFind?        FindExpression     { get; protected internal set; }
        public QUtilize?     UtilizeExpression  { get; protected internal set; }
        public List<QFilter> Scope              { get; protected internal set; }
        public QUtilize?     UtilizeScope       { get; protected internal set; }
        public List<QAssign> Assignments        { get; protected internal set; }
        public QUtilize?     UtilizeAssignments { get; protected internal set; }
        public QApply?       MacroLabel         { get; protected internal set; }
        public QSettings     Settings           { get; protected internal set; }
        public QueryResult   Results            { get; protected internal set; }

        private QSelectionCriteria(QContext env, QueryResult results, string text, string verb, QApply? applyLabel = null) : base(env, text, verb)
        {
            this.FindExpression = null;
            this.Assignments = new();
            this.Scope = new();
            this.MacroLabel = applyLabel;
            this.Settings = new QSettings(env.GlobalSettings);
            this.Results = results;

            this.UtilizeExpression = null;
            this.UtilizeScope = null;
            this.UtilizeAssignments = null;
    }
        public static QSelectionCriteria CreateSegment(QContext env, QueryResult results, Parsed elements, QApply? applyLabel = null)
        {
            var segment = new QSelectionCriteria(env, results, elements.text, elements.rule, applyLabel);
            Dictionary<string, SearchFilter> filters = new();

            // TO DO: Add Scope
            foreach (Parsed clause in elements.children)
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

            foreach (Parsed clause in elements.children)
            {
                if (clause.rule.Equals("expression"))
                {
                    if (clause.children.Length == 1)
                    {
                        var expression = clause.children[0];

                        if (expression.rule.Equals("search", StringComparison.InvariantCultureIgnoreCase))
                        {
                            segment.FindExpression = QFind.Create(env, segment, filters, expression.text, clause.children);
                        }
                        else if (expression.rule.Equals("utilization_full", StringComparison.InvariantCultureIgnoreCase))
                        {
                            var invocation = QUtilize.Create(env, clause.text, clause.children);
                            if (invocation != null)
                            {
                                segment.FindExpression = invocation.Expression;
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
        }
        internal void ConditionallyUpdateSpanToFragmentCount()
        {
            if (this.FindExpression != null && this.FindExpression.Settings.SearchSpan != 0)
            {
                UInt16 fragCnt = (UInt16)this.FindExpression.Fragments.Count;
                if (fragCnt > this.FindExpression.Settings.SearchSpan)
                {
                    this.Settings.Span.Update(fragCnt);
                }
            }
        }
    }
}