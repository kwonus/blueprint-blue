namespace Blueprint.Blue
{
    using YamlDotNet.Serialization.NamingConventions;
    using YamlDotNet.Serialization;
    using System.Text;
    using System.IO;
    using Blueprint.Model.Implicit;
    using Pinshot.Blue;
    using System;
    using BlueprintBlue.Model;

    public class ExpandableHistory: ExpandableInvocation
    {
        public QID Id;
        public override UInt32 GetDateNumeric() // 16 Feb 2024
        {
            return (UInt32) ((this.Id.year * 100 * 100) + (this.Id.month * 100) + this.Id.day);
        }
        public override string Tag
        {
            get
            {
                return this.Id.ToString();
            }
            protected set
            {
                this.Id = new QID(value);
            }
        }
        public ExpandableHistory(): base()
        {
            this.Id = null;
        }

        public ExpandableHistory(string rawText, QSelectionCriteria statement) : base(rawText, statement)
        {
            this.Id = new(seq:null);
        }
        public ExpandableHistory(string rawText, QUtilize invocation) : base(rawText, invocation)
        {
            this.Tag = invocation.Tag;
        }
        public static ExpandableHistory? Deserialize(string? tag = null, string? yaml = null)
        {
            try
            {
                string file = !string.IsNullOrEmpty(yaml) ? yaml : string.Empty;
                if (string.IsNullOrEmpty(file) && !string.IsNullOrEmpty(tag))
                {
                    QID id = new QID(tag);
                    file = id.AsYamlPath();
                }

                if (File.Exists(file))
                {
                    using (StreamReader sr = new StreamReader(file))
                    {
                        try
                        {
                            string text = sr.ReadToEnd();
                            var input = new StringReader(text);
                            var deserializer = new DeserializerBuilder()/*.WithNamingConvention(PascalCaseNamingConvention.Instance)*/.Build();
                            var item = deserializer.Deserialize<ExpandableHistory>(input);
                            return item;
                        }
                        catch (Exception ex)
                        { 
                            ;
                        }
                    }
                }
            }
            catch
            {
                ;
            }
            return null;
        }

        public bool Serialize()
        {
            string yaml = this.Id.AsYamlPath();
            string folder = Path.GetDirectoryName(yaml);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            try
            {
                YamlDotNet.Serialization.Serializer serializer = new();

                using (var stream = new FileStream(yaml, FileMode.Create))
                {
                    // StreamWriter object that writes UTF-8 encoded text
                    using (var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: false))
                    {
                        serializer.Serialize(writer, this);
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
