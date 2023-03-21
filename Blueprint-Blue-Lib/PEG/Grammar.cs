using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Pinshot.PEG
{
    [DataContract]
    public class RootParse
    {
        [DataMember]
        public string input { set; get; }
        [DataMember]
        public Parsed[] result { set; get; }
        [DataMember]
        public string error { set; get; }

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
    public class QuelleStatement
    {
        public string command { get; private set; }

        public QuelleStatement(string stmt)
        {
            this.command = stmt;
        }
    }
}
