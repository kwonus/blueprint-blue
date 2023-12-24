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
        public QPrint?  LimitDirective  { get; internal set; }
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
                Parsed[] segments = stmt.children;

                for (int s = 0; s < segments.Length; s++)
                {
                    Parsed segment = segments[s];

                    QApply? macroLabel = null;
                    if ((segment.children.Length >= 1) && segment.children[0].rule.StartsWith("elements", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var elements = segment.children[0];
                        if ((segment.children.Length == 2) && segment.children[1].rule.StartsWith("apply_macro_", StringComparison.InvariantCultureIgnoreCase))
                        {
                            var macro = segment.children[1];
                            macroLabel = QApply.Create(context, segment.text, macro);
                        }
                        QCommandSegment seg = QCommandSegment.CreateSegment(context, elements, macroLabel);
                        valid = (seg != null);
                        if (valid)
                            implicits.Segments.Add(seg);
                    }
                }
            }
            return valid ? implicits : null;
        }
    }
}