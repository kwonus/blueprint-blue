namespace Pinshot.PEG
{
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;

    [DataContract]
    public class RootParse
    {
        [DataMember]
        public required string   input  { set; get; }
        [DataMember]
        public required Parsed[] result { set; get; }
        [DataMember]
        public required string   error  { set; get; }

        public static (RootParse root, bool ok) Create(Stream stream)
        {
            try
            {
                var serializer = new DataContractJsonSerializer(typeof(RootParse));
                var root = serializer.ReadObject(stream);
                if (root != null)
                    return ((RootParse) root, true);
            }
            catch
            {
                ;
            }
            return (new RootParse() { error = "Exception thrown during deserialization", input = "", result = new Parsed[0] }, false);
        }
    }
    [DataContract]
    public class Parsed
    {
        [DataMember]
        public string rule { set; get; }
        [DataMember]
        public string text { set; get; }
        [DataMember]
        public Parsed[] children { set; get; }
    }

    [DataContract]
    public class RawParseResult
    {
        [DataMember]
        public required string input  { set; get; }
        [DataMember]
        public required string result { set; get; }
        [DataMember]
        public required string error  { set; get; }

        public RawParseResult()
        {

        }

        public static (RawParseResult root, bool ok) Create(Stream stream)
        {
            try
            {
                var serializer = new DataContractJsonSerializer(typeof(RawParseResult));
                var root = serializer.ReadObject(stream);
                if (root != null)
                    return ((RawParseResult) root, true);
            }
            catch
            {
                ;
            }
            return (new RawParseResult() { error = "Exception thrown during deserialization", input = "", result = "" }, false);
        }
    }
    public class QuelleStatement
    {
        public string command { get; private set; }

        public QuelleStatement(string stmt)
        {
            this.command = stmt;
        }
    }
}
