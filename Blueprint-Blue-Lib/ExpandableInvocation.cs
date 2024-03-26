namespace Blueprint.Blue
{
    using System;
    using System.IO;
    using System.Text;
    using System.Text.Json.Serialization;
    using YamlDotNet.Serialization;

    public class ExpandableInvocation
    {
        public long Time                    { get; protected set; }
        public string? Expression           { get; protected set; }
        public string Statement             { get; protected set; }

        public List<string> Filters         { get; protected set; }
        public Dictionary<string, string> Settings { get; protected set; }


        public ExpandableInvocation()
        {
            this.Statement = string.Empty;
            this.Time = 0;
            this.Expression = null;
            this.Filters = new();
            this.Settings = new();
        }

        public ExpandableInvocation(string rawText, QSelectionCriteria statement)
        {
            this.Statement = rawText;
            this.Time = DateTimeOffset.Now.ToFileTime();
            this.Expression = statement.SearchExpression != null ? statement.SearchExpression.Expression : null;
            this.Filters = new();
            foreach (QFilter filter in statement.Scope)
            {
                this.Filters.Add(filter.Filter);
            }
            this.Settings = statement.Settings.AsMap();
        }
        public ExpandableInvocation(string rawText, QUtilize invocation)
        {
            this.Statement = rawText;
            this.Time = 0;
            this.Expression = null;
            this.Filters = new();
            this.Settings = invocation.Settings.AsMap();
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

            QSettings settings = new QSettings(this.Settings);
            string variables = settings.AsMarkdown(showDefaults: true, showExtendedSettings: false);

            markdown.AppendLine(variables);

            return markdown.ToString();
        }
    }
}
