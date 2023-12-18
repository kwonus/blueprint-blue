namespace Blueprint.Blue
{
    using Pinshot.PEG;
    using System.Text;
    using System.Collections.Generic;
    using System.Linq;
    using System;
    using YamlDotNet.Serialization;
    using System.Text.Json.Serialization;
    using BlueprintBlue.Model;
    using BlueprintBlue.Model.Implicit;
    using System.Runtime.CompilerServices;
    using static AVXLib.Framework.Numerics;
    using System.Linq.Expressions;

    public class QImplicitCommands
    {
        [JsonIgnore]
        [YamlIgnore]
        public QContext Context { get; set; }

        public QExport? ExportDirective { get; internal set; }
        public QLimit?  LimitDirective  { get; internal set; }
        public QApply?  MacroDefinition { get; internal set; }

        public List<QCommandSegment> Segments { get; internal set; }

        public List<QFilter> Filters { get; internal set; }

        private QImplicitCommands(QContext env, string stmtText)
        {
            this.Context = env;
            this.ExportDirective = null;
            this.LimitDirective  = null;
            this.MacroDefinition = null;

            this.Segments = new();
        }

        public static QImplicitCommands? Create(QContext context, Parsed stmt, QStatement diagnostics)
        {
            bool valid = false;
            var implicits = new QImplicitCommands(context, stmt.text);

            if (stmt.rule.Equals("implicits", StringComparison.InvariantCultureIgnoreCase) && (stmt.children.Length >= 1))
            {
                QApply? macroLabel = null;
                uint invocation_cnt = 0;

                bool another = false;
                Parsed child = stmt.children[0];
                int cnt = stmt.children.Length;
                int idx = 1;
                Parsed segment = child;
                do
                { 
                    if (segment.rule.Equals("macro_segment", StringComparison.InvariantCultureIgnoreCase) && (stmt.children.Length == 1))
                    {
                        segment = child.children[0];
                        macroLabel = QApply.Create(context, segment.text, child.children);
                        cnt = 0;
                    }
                    else if (segment.rule.Equals("additional_segment", StringComparison.InvariantCultureIgnoreCase) && (segment.children.Length == 1))
                    {
                        segment = segment.children[0];
                    }

                    if (segment.rule.Equals("segment", StringComparison.InvariantCultureIgnoreCase))
                    {
                        QCommandSegment seg = QCommandSegment.CreateSegment(context, segment, macroLabel);
                        macroLabel = null;
                        implicits.Segments.Add(seg);
                        foreach (Parsed clause in segment.children)
                        {
                            if (clause.rule.Equals("expression"))
                            {
                                if (clause.children.Length == 1)
                                {
                                    var expression = clause.children[0];

                                    if (expression.rule.Equals("search", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        seg.SearchExpression = QFind.Create(context, expression.text, clause.children);
                                    }
                                    else if (expression.rule.Equals("invoke_full", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        var invocation = QInvoke.Create(context, clause.text, clause.children, partial:false);
                                        if (invocation != null)
                                        {
                                            seg.SearchExpression = invocation.SearchExpression;
                                            foreach (var assignment in invocation.Assignments)
                                            {
                                                seg.Assignments.Add(assignment);
                                            }
                                            foreach (var filter in invocation.Filters)
                                            {
                                                seg.Filters.Add(filter);
                                            }
                                            seg.Invocations.Add(invocation);
                                            invocation_cnt++;
                                        }
                                    }
                                }
                            }
                            else if (clause.rule.Equals("variable"))
                            {
                                if (clause.children.Length == 1)
                                {
                                    var variable = clause.children[0];

                                    if (variable.rule.Equals("opt", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        seg.SearchExpression = QFind.Create(context, variable.text, clause.children);
                                    }
                                    else if (variable.rule.Equals("opt", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        var filter = QFilter.Create(context, variable.text, clause.children);
                                        if (filter != null)
                                        {
                                            seg.Filters.Add(filter);
                                        }
                                    }
                                    else if (variable.rule.Equals("invoke_partial", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        var invocation = QInvoke.Create(context, clause.text, clause.children);
                                        if (invocation != null)
                                        {
                                            foreach (var assignment in invocation.Assignments)
                                            {
                                                seg.Assignments.Add(assignment);
                                            }
                                            foreach (var filter in invocation.Filters)
                                            {
                                                seg.Filters.Add(filter);
                                            }
                                            seg.Invocations.Add(invocation);
                                            invocation_cnt++;
                                        }
                                    }
                                }
                            }
                        }

                        if (idx < cnt)
                        {
                            segment = stmt.children[idx++];
                            another = (segment.rule.Equals("additional_segment", StringComparison.InvariantCultureIgnoreCase));
                        }
                    }
                }   while (another);

                context.InvocationCount += invocation_cnt;

                for (/**/; idx < cnt; idx++)
                {
                    var singleton = child.children[idx];

                    if (segment.rule.Equals("print", StringComparison.InvariantCultureIgnoreCase))
                    {
                        ;
                    }
                    else if (segment.rule.Equals("export", StringComparison.InvariantCultureIgnoreCase))
                    {
                        ;
                    }
                }
            }
            return valid ? implicits : null;
        }
    }
}