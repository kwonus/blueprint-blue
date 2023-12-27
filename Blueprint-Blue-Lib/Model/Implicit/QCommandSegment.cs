namespace Blueprint.Blue
{
    using AVSearch.Model.Expressions;
    using AVSearch.Model.Results;
    using Blueprint.Model.Implicit;
    using Pinshot.PEG;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text.Json.Serialization;
    using YamlDotNet.Serialization;
    using static AVXLib.Framework.Numerics;

    public class QCommandSegment : QCommand, ICommand
    {
        public QFind? SearchExpression      { get; internal set; }
        public List<QAssign>  Assignments   { get; internal set; }
        public List<QInvoke>  Invocations   { get; internal set; }
        public QApply?        MacroLabel    { get; internal set; }
        public QSettings      Settings      { get; protected set; }
        public QueryResult    Results       { get; protected set; }

        private QCommandSegment(QContext env, QueryResult results, string text, string verb, QApply? applyLabel = null) : base(env, text, verb)
        {
            this.Settings = new QSettings(env.GlobalSettings);
            this.Results = results;
            this.SearchExpression = null;
            this.Assignments = new();
            this.Invocations = new();
            this.MacroLabel = applyLabel;
        }
        public static QCommandSegment? CreateSegment(QContext env, QueryResult results, Parsed elements, QApply? applyLabel = null)
        {
            var segment = new QCommandSegment(env, results, elements.text, elements.rule, applyLabel);
            Dictionary<string, SearchFilter> filters = new();
            foreach (Parsed clause in elements.children)
            {
                if (clause.rule.Equals("element"))
                {
                    if (clause.children.Length == 1)
                    {
                        var variable = clause.children[0];

                        if (variable.rule.Equals("var_opt", StringComparison.InvariantCultureIgnoreCase))
                        {
                            QAssign? assignment = QVariable.CreateAssignment(env, variable.text, variable.children);
                            if (assignment != null)
                            {
                                segment.Assignments.Add(assignment);
                                segment.Settings.Assign(assignment);
                            }
                        }
                        else if (variable.rule.Equals("invoke_partial", StringComparison.InvariantCultureIgnoreCase))
                        {
                            var invocation = QInvoke.Create(env, clause.text, clause.children);
                            if (invocation != null)
                            {
                                segment.Settings.CopyFrom(invocation.Settings);
                                QFind.AddFilters(filters, invocation.Filters);

                                segment.Invocations.Add(invocation);
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
                            segment.SearchExpression = QFind.Create(env, segment.Settings, filters, expression.text, clause.children);
                        }
                        else if (expression.rule.Equals("invoke_full", StringComparison.InvariantCultureIgnoreCase))
                        {
                            var invocation = QInvoke.Create(env, clause.text, clause.children, partial: false);
                            if (invocation != null)
                            {
                                segment.SearchExpression = invocation.Expression;
                                segment.Settings.CopyFrom(invocation.Settings);
                                QFind.AddFilters(filters, invocation.Filters);

                                segment.Invocations.Add(invocation);
                            }
                        }
                    }
                }
            }
            return segment;
        }
    }
}