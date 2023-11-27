namespace Blueprint.Blue
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Text.Json;
    using XBlueprintBlue;

    public interface ICommand
    {
        string Text { get; }
        string Verb { get; }
        QContext Context { get; }
        bool IsExplicit { get; }

        string Expand();
        List<string> AsYaml();

        static List<string> JsonSerializer(object obj)
        {
            JsonSerializerOptions options = new() { WriteIndented = true };

            List<string> result = new();
            string json = System.Text.Json.JsonSerializer.Serialize(obj, options);

            string[] lines = json.Split([Environment.NewLine], StringSplitOptions.None);

            foreach (string line in lines)
            {
                var trimmed = line.TrimEnd();
                if (!string.IsNullOrEmpty(trimmed))
                    result.Add(trimmed);
            }
            return result;
        }

        static List<string> YamlSerializer(object obj)
        {
            List<string> result = new();
            YamlDotNet.Serialization.Serializer serializer = new();

            using (var stream = new MemoryStream())
            {
                // StreamWriter object that writes UTF-8 encoded text to the MemoryStream.
                using (var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true))
                {
                    serializer.Serialize(writer, obj);
                    writer.Flush(); // Make sure all data is written to the MemoryStream.
                }
                // Set the position of the MemoryStream back to the beginning.
                stream.Position = 0;

                // StreamReader object that reads UTF-8 encoded text from the MemoryStream.
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    for (string? line = reader.ReadLine(); line != null; line = reader.ReadLine())
                    {
                        var trimmed = line.TrimEnd();
                        if (!string.IsNullOrEmpty(trimmed))
                            result.Add(trimmed);
                    }
                }
            }
            return result;
        }
    }

    public class QCommand
    {
        public string Text { get; set; }
        public string Verb { get; set; }
        public QContext Context { get; set; }
        public QHelpDoc HelpDoc { get => QHelpDoc.GetDocument(this.Verb); }

        protected QCommand(QContext context, string text, string verb)
        {
            this.Context = context;
            this.Text = text;
            this.Verb = verb;
        }
        virtual public List<string> AsYaml()
        {
            return ICommand.YamlSerializer(this);
        }
    }
}