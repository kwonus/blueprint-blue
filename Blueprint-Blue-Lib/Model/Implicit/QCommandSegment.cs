namespace Blueprint.Blue
{
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

        private QCommandSegment(QContext env, string text, string verb, QApply? applyLabel = null) : base(env, text, verb)
        {
            this.Settings = new QSettings(env.GlobalSettings);

            this.SearchExpression = null;
            this.Assignments = new();
            this.Invocations = new();
            this.MacroLabel = applyLabel;
        }
        public static QCommandSegment? CreateSegment(QContext env, Parsed elements, QApply? applyLabel = null)
        {
            var segment = new QCommandSegment(env, elements.text, elements.rule, applyLabel);

            foreach (Parsed clause in elements.children)
            {
                if (clause.rule.Equals("expression"))
                {
                    if (clause.children.Length == 1)
                    {
                        var expression = clause.children[0];

                        if (expression.rule.Equals("search", StringComparison.InvariantCultureIgnoreCase))
                        {
                            segment.SearchExpression = QFind.Create(env, segment.Settings, expression.text, clause.children);
                        }
                        else if (expression.rule.Equals("invoke_full", StringComparison.InvariantCultureIgnoreCase))
                        {
                            var invocation = QInvoke.Create(env, clause.text, clause.children, partial: false);
                            if (invocation != null)
                            {
                                segment.SearchExpression = invocation.Expression;
                                segment.Settings.CopyFrom(invocation.Settings);

                                segment.Invocations.Add(invocation);
                            }
                        }
                    }
                }
                else if (clause.rule.Equals("element"))
                {
                    if (clause.children.Length == 1)
                    {
                        var variable = clause.children[0];

                        if (variable.rule.Equals("var_opt", StringComparison.InvariantCultureIgnoreCase))
                        {
                            segment.SearchExpression = QFind.Create(env, segment.Settings, variable.text, clause.children);
                        }
                        else if (variable.rule.Equals("invoke_partial", StringComparison.InvariantCultureIgnoreCase))
                        {
                            var invocation = QInvoke.Create(env, clause.text, clause.children);
                            if (invocation != null)
                            {
                                segment.Settings.CopyFrom(invocation.Settings);
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