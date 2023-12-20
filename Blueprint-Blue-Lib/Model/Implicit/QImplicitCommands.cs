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
    using static System.Net.Mime.MediaTypeNames;

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
                Parsed segments = stmt.children[0];

                if (segments.rule.Equals("segments", StringComparison.InvariantCultureIgnoreCase))
                {
                    for (int s = 0; s < segments.children.Length; s++)
                    {
                        Parsed segment = segments.children[s];

                        QApply? macroLabel = null;
                        if ((segment.children.Length == 2) && segment.children[1].rule.StartsWith("apply_macro_", StringComparison.InvariantCultureIgnoreCase))
                        {
                            var macro = segment.children[1];
                            macroLabel = QApply.Create(context, segment.text, macro);
                        }
                        QCommandSegment seg = QCommandSegment.CreateSegment(context, segment, macroLabel);

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
                                        }
                                    }
                                }
                            }
                            else if (clause.rule.Equals("element"))
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
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
                for (int additional = 1; additional < stmt.children.Length; additional++)
                {
                    var clause = stmt.children[additional];
                    if (clause.rule.Equals("export", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (implicits.ExportDirective == null) // these are implicit singletons; grammar should enforce this, but enforce it here too
                            implicits.ExportDirective = QExport.Create(context, clause.text, clause.children);
                    }
                    else if (clause.rule.Equals("print", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (implicits.LimitDirective == null) // these are implicit singletons; grammar should enforce this, but enforce it here too
                            implicits.LimitDirective = QLimit.Create(context, clause.text, clause.children);
                    }
                }
            }
            return valid ? implicits : null;
        }
    }
}