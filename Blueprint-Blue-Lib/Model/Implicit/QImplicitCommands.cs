namespace Blueprint.Blue
{
    using Pinshot.PEG;
    using System.Collections.Generic;
    using System;
    using YamlDotNet.Serialization;
    using System.Text.Json.Serialization;
    using Blueprint.Model.Implicit;
    using static AVXLib.Framework.Numerics;
    using AVSearch.Model.Results;
    using System.Runtime.CompilerServices;
    using AVSearch.Model.Expressions;

    public class QImplicitCommands
    {
        [JsonIgnore]
        [YamlIgnore]
        public QContext Context { get; set; }

        public QueryResult Results { get; set; }

        public QExport? ExportDirective { get; internal set; }
        public QPrint?  LimitDirective  { get; internal set; }

        public List<QCommandSegment> Segments { get; internal set; }

        [JsonIgnore]
        [YamlIgnore]
        public IEnumerable<SearchExpression> Expressions
        {
            get
            {
                foreach (QCommandSegment seg in this.Segments)
                {
                    if (seg.FindExpression != null)
                        yield return seg.FindExpression;
                }
            }
        }

        public (bool ok, QueryResult query) Execute() // TO DO: Quoted searches do not appear to be parsing correctly
        {
            bool executed = false;
            foreach (var segment in this.Segments)
            {
                if (segment.FindExpression != null)
                {
                    executed = executed || (segment.FindExpression.Scope.Count == 0
                        ? this.Search(segment.FindExpression)
                        : this.SearchWithScope(segment.FindExpression));
                }
            }
            foreach (SearchExpression exp in this.Expressions)
            {
                for (byte b = 1; b <= 66; b++)
                    if (exp.Books.ContainsKey(b) && (exp.Books[b].Chapters.Count == 0))
                        exp.Books.Remove(b);
            }
            return (executed, this.Results);
        }
        private bool Search(QFind search)
        {
            bool result = search.Fragments.Count > 0;

            if (result)
            {
                search.AddScope(0);
                foreach (var book in search.Books.Values)
                {
                    result = book.Search(search) || result;
                }
            }
            return result;
        }
        private bool SearchWithScope(QFind search)
        {
            bool result = search.Fragments.Count > 0;

            if (result)
            {
                foreach (var filter in search.Scope.Values)
                {
                    search.AddScope(filter);
                }
                foreach (var book in search.Books.Values)
                {
                    result = result || book.Search(search);
                }
            }
            return result;
        }

        private QImplicitCommands(QContext env, string stmtText)
        {
            this.Context = env;
            this.ExportDirective = null;
            this.LimitDirective  = null;

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
                        QCommandSegment seg = QCommandSegment.CreateSegment(context, implicits.Results, elements, macroLabel);
                        valid = (seg != null);
                        if (seg != null)
                            implicits.Segments.Add(seg);
                    }
                }
            }
            implicits.Results = new QueryResult(implicits.Expressions);

            return valid ? implicits : null;
        }
    }
}