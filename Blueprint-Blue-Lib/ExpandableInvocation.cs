namespace Blueprint.Blue
{
    using System;
    using System.IO;
    using System.Text;
    using System.Text.Json.Serialization;
    using YamlDotNet.Serialization;

    public enum MacroComponents
    {
        Ignore = 0,
        SettingsOnly = 1,
        ScopingOnly = 2,
        ExpressionOnly = 4,
        FullMacro = 7,
    }
    public class ExpandableInvocation
    {
        public long Time                    { get; protected set; }
        public string? Expression           { get; protected set; }
        public string Statement             { get; protected set; }

        public List<string>? Filters        { get; protected set; }
        public QSettings Settings           { get; protected set; }

        [JsonIgnore]
        [YamlIgnore]
        public MacroComponents Parts        { get; protected set; }

        public ExpandableInvocation(MacroComponents parts)
        {
            this.Statement = string.Empty;
            this.Time = 0;
            this.Expression = null;
            this.Filters = null;
            this.Settings = new QSettings();
            this.Parts = parts;
        }

        public ExpandableInvocation(string rawText, QSelectionCriteria statement, MacroComponents parts)
        {
            this.Statement = rawText;
            this.Time = DateTimeOffset.Now.ToFileTime();
            this.Expression = statement.SearchExpression != null ? statement.SearchExpression.Expression : null;
            this.Settings = new QSettings(statement.Settings);
            this.Parts = parts;
            if (statement.SearchExpression != null && statement.SearchExpression.Scope.Count > 0 )
            {
                this.Filters = new();
                foreach (var filter in statement.SearchExpression.Scope.Values)
                {
                    this.Filters.Add(((QFilter)filter).Filter);
                }
            }
        }
        public ExpandableInvocation(string rawText, QUtilize invocation, MacroComponents parts)
        {
            this.Statement = rawText;
            this.Time = 0;
            this.Expression = null;
            this.Filters = null;
            this.Settings = invocation.Settings;
            this.Parts = parts;

            // TODO:
            // we need to read info from the in-memory macro-list or history-list
        }
        public static bool ExpandInvocation(ref QUtilize invocation)
        {
            // TODO:
            // 1) we need to read info from the in-memory macro-list or history-list
            // 2) we need to pass Expression and Filters to the pinshot parser for re-instantiations
            return false;
        }

        public DateTime GetDateTime()
        {
            DateTimeOffset offset = DateTimeOffset.FromFileTime(this.Time);
            return offset.DateTime;
        }

        public static string YamlSerializer(object obj)
        {
            try
            {
                YamlDotNet.Serialization.Serializer serializer = new();

                using (var stream = new MemoryStream())
                {
                    // StreamWriter object that writes UTF-8 encoded text to the MemoryStream.
                    using (var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true))
                    {
                        serializer.Serialize(writer, obj);
                        writer.Flush(); // Make sure all data is written to the MemoryStream.
                    }
                    stream.Position = 0;

                    // StreamReader object that reads UTF-8 encoded text from the MemoryStream.
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch
            {
                ;
            }
            return string.Empty;
        }
        public static bool YamlSerializer(string yaml, object obj)
        {
            try
            {
                YamlDotNet.Serialization.Serializer serializer = new();

                using (var stream = new FileStream(yaml, FileMode.Create))
                {
                    // StreamWriter object that writes UTF-8 encoded text
                    using (var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: false))
                    {
                        serializer.Serialize(writer, obj);
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

        public string AsMarkdown()
        {
            long t = this.Time;
            string time = DateTimeOffset.FromFileTime(t).DateTime.ToString();

            StringBuilder markdown = new StringBuilder(384);
            markdown.AppendLine("| Property | Value |\n" + "| ---------- | ------------ |");

            markdown.Append("| Created | ");
            markdown.Append(time);
            markdown.AppendLine(" |");

            if (!string.IsNullOrWhiteSpace(this.Expression))
            {
                markdown.Append("| Expression | ");
                markdown.Append(this.Expression);
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

            this.Settings.AsMarkdown(showDefaults: true, showExtendedSettings: false);

            return markdown.ToString();
        }
    }
}
