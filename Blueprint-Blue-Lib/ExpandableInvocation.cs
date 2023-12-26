namespace Blueprint.Blue
{
    using Blueprint.Blue;
    using System;
    using System.Text;
    using YamlDotNet.RepresentationModel;
    using YamlDotNet.Serialization.NamingConventions;
    using YamlDotNet.Serialization;
    using System.ComponentModel.DataAnnotations;

    public class ExpandableInvocation
    {
        public long Time                    { get; set; }
        public string? Expression           { get; set; }
        public List<string>? Filters        { get; set; }
        public QSettings Settings           { get; set; }

        public ExpandableInvocation()
        {
            this.Time = 0;
            this.Expression = null;
            this.Filters = null;
            this.Settings = new QSettings();
        }

        public ExpandableInvocation(QCommandSegment statement)
        {
            this.Time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            this.Expression = statement.SearchExpression != null ? statement.SearchExpression.Expression : null;
            this.Settings = new QSettings(statement.Settings);
            if (statement.SearchExpression != null && statement.SearchExpression.Filters.Count > 0 )
            {
                this.Filters = new();
                foreach (var filter in statement.SearchExpression.Filters.Values)
                {
                    this.Filters.Add(((QFilter)filter).Filter);
                }
            }
        }
        public ExpandableInvocation(QInvoke invocation)
        {
            this.Time = 0;
            this.Expression = null;
            this.Filters = null;
            this.Settings = invocation.Settings;

            // TODO:
            // we need to read info from the in-memory macro-list or history-list
        }
        public static bool ExpandInvocation(ref QInvoke invocation)
        {
            // TODO:
            // 1) we need to read info from the in-memory macro-list or history-list
            // 2) we need to pass Expression and Filters to the pinshot parser for re-instantiations
            return false;
        }

        public DateTime GetDateTime()
        {
            DateTimeOffset offset = DateTimeOffset.FromUnixTimeMilliseconds(this.Time);
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
    }
}
