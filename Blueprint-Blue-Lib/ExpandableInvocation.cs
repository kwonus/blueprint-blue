namespace Blueprint.Blue
{
    using AVSearch.Interfaces;
    using AVSearch.Model.Expressions;
    using AVXLib;
    using Blueprint.Model.Implicit;
    using BlueprintBlue.Model;
    using Pinshot.Blue;
    using Pinshot.PEG;
    using System;
    using System.Diagnostics.Metrics;
    using System.IO;
    using System.Linq.Expressions;
    using System.Text;
    using System.Text.Json.Serialization;
    using YamlDotNet.Serialization;

    public class ExpandableInvocation
    {
        private string unused;
        public virtual string Tag
        {
            get
            {
                return string.Empty;
            }
            protected set
            { 
                ;
            }
        }
        public string Created            { get; protected set; }
        public virtual UInt32 GetDateNumeric() // 16 Feb 2024
        {
            if (this.Created.Length == 11)
            {
                string month = this.Created.Substring(3, 3);
                for (UInt32 m = 1; m <= 12; m++)
                {
                    if (QID.Months[m].Equals(month, StringComparison.InvariantCultureIgnoreCase))
                    {
                        UInt32 numeric = 0;
                        try
                        {
                            numeric = (UInt32.Parse(this.Created.Substring(7)) * 100 * 100)
                                    + (m * 100)
                                    + UInt32.Parse(this.Created.Substring(0, 2));
                            return numeric;
                        }
                        catch
                        {
                            return 0;
                        }
                    }
                }
            }
            return 0;
        }
        public ParsedExpression? Expression   { get; protected set; }
        public string Statement          { get; protected set; }
        public HashSet<string> Filters   { get; protected set; } // these are used for display
        public Dictionary<byte, ScopingFilter> Scope;
        public Dictionary<string, string> Settings { get; protected set; }

        public static ExpandableInvocation? Deserialize(QUtilize utilization)
        {
            if (utilization.TagType == TagType.Macro)
            {
                return ExpandableMacro.Deserialize(utilization.Tag);
            }
            if (utilization.TagType == TagType.History)
            {
                return ExpandableHistory.Deserialize(utilization.Tag);
            }
            return null;
        }

        public ExpandableInvocation()
        {
            this.Statement = string.Empty;
            this.Created = string.Empty;
            this.Expression = null;
            this.Scope = new();
            this.Filters = new();
            this.Settings = new();
        }

        public ExpandableInvocation(string rawText, QSelectionCriteria statement)
        {
            this.Statement = rawText;
            this.Created = QID.Today();
            this.Expression = statement.SearchExpression != null ? statement.SearchExpression.Expression : null;
            this.Scope = new();
            this.Filters = new();

            foreach (QFilter filter in statement.Scope)
            {
                if (!this.Filters.Contains(filter.RawText))
                {
                    this.Filters.Add(filter.RawText);
                }
                IEnumerable<ScopingFilter>? results = ScopingFilter.Create(filter.Textual, filter.Ranges);
                if (results != null)
                {
                    foreach (ScopingFilter result in results)
                    {
                        if (!this.Scope.ContainsKey(result.Book))
                            this.Scope[result.Book] = result;
                        else
                            this.Scope[result.Book].Ammend(result);
                    }
                }
            }
            this.Settings = statement.Settings.AsMap();
        }
        public ExpandableInvocation(string rawText, QUtilize invocation)
        {
            this.Statement = rawText;
            this.Created = QID.Today();
            this.Expression = null;
            this.Scope = new();
            this.Filters = new();
            this.Settings = invocation.Settings.AsMap();
        }

        public static bool YamlSerializer(Dictionary<Int64, ExpandableHistory> history)
        {
            try
            {
                YamlDotNet.Serialization.Serializer serializer = new();

                using (var stream = new FileStream(QContext.HistoryPath, FileMode.Create))
                {
                    // StreamWriter object that writes UTF-8 encoded text
                    using (var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: false))
                    {
                        serializer.Serialize(writer, history);
                        writer.Flush(); // Make sure all data is written to the MemoryStream.
                    }
                    return true;
                }
            }
            catch
            {
                ;
            }
            return false;
        }
        private static char[] newlineDelimiters = new char[] { '\r', '\n' };

        public static bool JsonSerializer(string json, object obj)
        {
            try
            {
                string output = System.Text.Json.JsonSerializer.Serialize(obj);

                using (var stream = new FileStream(json, FileMode.Create))
                {
                    // StreamWriter object that writes UTF-8 encoded text
                    using (var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: false))
                    {
                        writer.Write(output);
                        writer.Flush(); // Make sure all data is written to the MemoryStream.
                    }
                    return true;
                }
            }
            catch
            {
                ;
            }
            return false;
        }
        public static string SerializeToYamlString(object obj)
        {
            try
            {
                YamlDotNet.Serialization.Serializer serializer = new();
                string output = serializer.Serialize(obj);
                return output;
            }
            catch
            {
                ;
            }
            return string.Empty;
        }
        public static string SerializeToJsonString(object obj)
        {
            try
            {
                string output = System.Text.Json.JsonSerializer.Serialize(obj);
                return output;
            }
            catch
            {
                ;
            }
            return string.Empty;
        }

        public string AsMarkdownOld()
        {
            StringBuilder markdown = new StringBuilder(384);
            markdown.AppendLine("| Property | Value |\n" + "| ---------- | ------------ |");

            markdown.Append("| Created | ");
            markdown.Append(this.Created);
            markdown.AppendLine(" |");

            if (this.Expression != null)
            {
                markdown.Append("| Expression | ");
                if (this.Expression.Ordered)
                    markdown.Append('"');
                markdown.Append(this.Expression);
                if (this.Expression.Ordered)
                    markdown.Append('"');
                markdown.AppendLine(" |");
            }
            if (this.Filters != null && this.Filters.Count > 0)
            {
                markdown.Append("| Filters | ");
                foreach (var filter in this.Filters)
                {
                    if (!string.IsNullOrWhiteSpace(filter))
                    {
                        markdown.Append(filter);
                        markdown.AppendLine(", ");
                    }
                }
                markdown.AppendLine(" |");
            }
            markdown.AppendLine();

            QSettings settings = new QSettings(this.Settings);
            string variables = settings.AsMarkdown(showDefaults: true, showExtendedSettings: false);

            markdown.AppendLine(variables);

            return markdown.ToString();
        }
        private void TagAsMarkdownEntry(StringBuilder table) => table.AppendLine(string.Format(AsMarkdownTableEntry, "tag", this.Tag));
        private void TimeAsMarkdownEntry(StringBuilder table) => table.AppendLine(string.Format(AsMarkdownTableEntry, "date", this.Created));
        private void ExpressionAsMarkdownEntry(StringBuilder table) => table.AppendLine(string.Format(AsMarkdownTableEntry, "expression", this.Expression != null ? this.Expression.ToString() : string.Empty));
        private void FiltersAsMarkdownEntry(StringBuilder table)
        {
            StringBuilder filters = new StringBuilder(128);

            int i = 0;
            foreach (var filter in this.Filters)
            {
                if (i++ > 0)
                    filters.AppendLine("br/>");
                filters.Append(filter);
            }
            if (i > 0)
                filters.AppendLine();

            table.AppendLine(string.Format(AsMarkdownTableEntry, "filters", filters.ToString()));
        }
        private void SettingsAsMarkdownEntry(StringBuilder table)
        {
            table.AppendLine(string.Format(AsMarkdownTableEntry, QSpan.Name, this.Settings[QSpan.Name]));
            table.AppendLine(string.Format(AsMarkdownTableEntry, QLexicalDomain.Name, this.Settings[QLexicalDomain.Name]));
            table.AppendLine(string.Format(AsMarkdownTableEntry, QLexicalDisplay.Name, this.Settings[QLexicalDisplay.Name]));
            table.AppendLine(string.Format(AsMarkdownTableEntry, QFormat.Name, this.Settings[QFormat.Name]));
            table.AppendLine(string.Format(AsMarkdownTableEntry, QSimilarityWord.Name, this.Settings[QSimilarityWord.Name]));
            table.AppendLine(string.Format(AsMarkdownTableEntry, QSimilarityLemma.Name, this.Settings[QSimilarityLemma.Name]));
        }
        private void TagAsHtmlEntry(StringBuilder table) => table.AppendLine(string.Format(AsHtmlTableEntry, "tag", this.Tag));
        private void TimeAsHtmlEntry(StringBuilder table) => table.AppendLine(string.Format(AsHtmlTableEntry, "date", this.Created));
        private void ExpressionAsHtmlEntry(StringBuilder table) => table.AppendLine(string.Format(AsHtmlTableEntry, "expression", this.Expression != null ? this.Expression.ToString() : string.Empty));
        private void FiltersAsHtmlEntry(StringBuilder table)
        {
            StringBuilder filters = new StringBuilder(128);

            int i = 0;
            foreach (var filter in this.Filters)
            {
                if (i++ > 0)
                    filters.AppendLine("br/>");
                filters.Append(filter);
            }
            if (i > 0)
                filters.AppendLine();

            table.AppendLine(string.Format(AsHtmlTableEntry, "filters", filters.ToString()));
        }
        private static string FiltersAsHtmlEntry(StringBuilder table, HashSet<string> Filters)
        {
            StringBuilder filters = new StringBuilder(128);

            int i = 0;
            foreach (var filter in Filters)
            {
                if (i++ > 0)
                    filters.AppendLine("br/>");
                filters.Append(filter);
            }
            if (i > 0)
                filters.AppendLine();

            return filters.ToString();
        }
        private void SettingsAsHtmlEntry(StringBuilder table)
        {
            table.AppendLine(string.Format(AsHtmlTableEntry, QSpan.Name, this.Settings[QSpan.Name]));
            table.AppendLine(string.Format(AsHtmlTableEntry, QLexicalDomain.Name, this.Settings[QLexicalDomain.Name]));
            table.AppendLine(string.Format(AsHtmlTableEntry, QLexicalDisplay.Name, this.Settings[QLexicalDisplay.Name]));
            table.AppendLine(string.Format(AsHtmlTableEntry, QFormat.Name, this.Settings[QFormat.Name]));
            table.AppendLine(string.Format(AsHtmlTableEntry, QSimilarityWord.Name, this.Settings[QSimilarityWord.Name]));
            table.AppendLine(string.Format(AsHtmlTableEntry, QSimilarityLemma.Name, this.Settings[QSimilarityLemma.Name]));
        }
        private const uint BulkTableHtmlColumnCount = 9;
        private static void AddBulkHeaderColumn(StringBuilder table, string name, uint position) // position is 1-based, not 0-based.
        {
            if (position == 1)
                table.Append("<tr style='background-color:#000000;'>");

            table.Append("<td>");
            table.Append(name);
            table.Append("</td>");

            if (position == BulkTableHtmlColumnCount)
                table.AppendLine("</tr>\n");
        }
        private static void AddBulkValueColumn(StringBuilder table, string value, uint position) // position is 1-based, not 0-based.
        {
            if (position == 1)
                table.Append("<tr style='color:white;'>");

            table.Append("<td>");
            table.Append(value);
            table.Append("</td>");

            if (position == BulkTableHtmlColumnCount)
                table.AppendLine("</tr>\n");
        }
        public static string AsBulkHtml(IEnumerable<ExpandableInvocation> items)
        {
            StringBuilder table = new StringBuilder(1024);
            table.AppendLine(ExpandableInvocation.AsHtmlTablePreamble);
            uint i = 1;
            AddBulkHeaderColumn(table, "Tag", i++);
            AddBulkHeaderColumn(table, "Created", i++);
            AddBulkHeaderColumn(table, "Original Statement", i++);
            AddBulkHeaderColumn(table, "Expression", i++);
            AddBulkHeaderColumn(table, "Filters", i++);
            AddBulkHeaderColumn(table, "Span", i++);
            AddBulkHeaderColumn(table, "Format", i++);
            AddBulkHeaderColumn(table, "Lexicon (search/render)", i++);
            AddBulkHeaderColumn(table, "Similarity (word/lemma)", i);

            foreach (ExpandableInvocation invocation in items)
            {
                if (invocation != null)
                {
                    i = 1;
                    AddBulkHeaderColumn(table, invocation.Tag, i++);
                    AddBulkHeaderColumn(table, invocation.Created, i++);
                    AddBulkHeaderColumn(table, invocation.Statement, i++);
                    string expression = invocation.Expression == null ? string.Empty : invocation.Expression.Ordered ? "\"" + invocation.Expression.Text + "\"" : invocation.Expression.Text;
                    AddBulkHeaderColumn(table, expression, i++);
                    AddBulkHeaderColumn(table, ExpandableInvocation.FiltersAsHtmlEntry(table, invocation.Filters), i++);
                    AddBulkHeaderColumn(table, invocation.Settings[QSpan.Name], i++);
                    AddBulkHeaderColumn(table, invocation.Settings[QFormat.Name], i++);

                    string lexRender = invocation.Settings[QLexicalDisplay.Name];
                    string lexSearch = invocation.Settings[QLexicalDomain.Name];

                    string similarityWord  = invocation.Settings[QSimilarityWord.Name];
                    string similarityLemma = invocation.Settings[QSimilarityLemma.Name];

                    AddBulkHeaderColumn(table, lexSearch + " / " + lexRender, 7);
                    AddBulkHeaderColumn(table, similarityWord + " / " + similarityLemma, 8);
                }
            }
            table.AppendLine(ExpandableInvocation.AsHtmlTablePostamble);

            return table.ToString();
        }
        public string AsHtml()
        {
            StringBuilder table = new StringBuilder(1024);

            table.Append(AsHtmlTableHeader);
            TagAsHtmlEntry(table);
            TimeAsHtmlEntry(table);
            ExpressionAsHtmlEntry(table);
            FiltersAsHtmlEntry(table);
            SettingsAsHtmlEntry(table);
            table.Append(AsHtmlTablePostamble);

            return table.ToString();
        }
        public string AsMarkdown()
        {
            StringBuilder table = new StringBuilder(1024);

            table.Append(AsMarkdownTableHeader);
            TagAsMarkdownEntry(table);
            TimeAsMarkdownEntry(table);
            ExpressionAsMarkdownEntry(table);
            FiltersAsMarkdownEntry(table);
            SettingsAsMarkdownEntry(table);

            return table.ToString();
        }
        protected static string AsMarkdownTableHeader  { get; private set; }
        protected static string AsMarkdownTableEntry   { get; private set; }
        protected static string AsHtmlTablePreamble    { get; private set; }
        protected static string AsHtmlTableHeader      { get; private set; }
        protected static string AsHtmlTableEntry       { get; private set; }
        protected static string AsHtmlTablePostamble   { get; private set; }

        /*
        UInt64  Id
        long    Time
        string? Expression
        string  Statement
        HashSet<string>             Filters
        Dictionary<string, string>  Settings
         */
        static ExpandableInvocation()
        {
            ExpandableInvocation.AsMarkdownTableHeader = "| Property | Value |\n" + "| ---------- | ------------ |";
            ExpandableInvocation.AsMarkdownTableEntry  = "| {0}     | {1}   |";

            ExpandableInvocation.AsHtmlTablePreamble    = "<html><body style='background-color:#252526;color:#FFFFFF;font-family:calibri,arial,helvetica;font-size:24px'>"
                                                        + "<br/><table border='1' align='center'>";
            ExpandableInvocation.AsHtmlTableHeader      = ExpandableInvocation.AsHtmlTablePreamble
                                                        + "<tr style='background-color:#000000;'><th style='text-align:left;'>Property</th><th style='text-align:left;'>Value</th></tr>";
            ExpandableInvocation.AsHtmlTableEntry       = "<tr style='color:white;'><td>{0}</td><td>{1}</td></tr>\"";
            ExpandableInvocation.AsHtmlTablePostamble   = "</table></body></html>";
        }
    }
}
