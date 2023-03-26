
namespace Pinshot.Blue
{
    using Pinshot.PEG;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization.Json;
    using System.Text;

    internal static class Pinshot_RustFFI
    {
        [DllImport("pin_shot_avx.dll", EntryPoint = "create_quelle_parse_raw")]
        internal static extern ParsedStatementHandle pinshot_blue_raw_parse(string stmt);
        [DllImport("pin_shot_avx.dll", EntryPoint = "create_quelle_parse")]
        internal static extern ParsedStatementHandle pinshot_blue_parse(string stmt);
        [DllImport("pin_shot_avx.dll", EntryPoint = "delete_quelle_parse")]
        internal static extern void pinshot_blue_free(IntPtr memory);
    }

    internal class ParsedStatementHandle : SafeHandle
    {
        public ParsedStatementHandle() : base(IntPtr.Zero, true) { }

        public override bool IsInvalid
        {
            get { return this.handle == IntPtr.Zero; }
        }

        public string AsString()
        {
            int len = 0;
            while (Marshal.ReadByte(handle, len) != 0) { ++len; }
            byte[] buffer = new byte[len];
            Marshal.Copy(handle, buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer);
        }

        protected override bool ReleaseHandle()
        {
            if (!this.IsInvalid)
            {
                Pinshot_RustFFI.pinshot_blue_free(handle);
            }
            return true;
        }
    }

    public class PinshotLib
    {
        public PinshotLib()
        {
            ;
        }
        public (string json, RootParse? root) Parse(string stmt)
        {
            using (ParsedStatementHandle handle = Pinshot_RustFFI.pinshot_blue_parse(stmt))
            {
                var result = handle.AsString();
                using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(result)))
                {
                    // Deserialization from JSON
                    DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(RootParse));
                    var root = (RootParse?)deserializer.ReadObject(ms);
                    return (result, root);
                }
            }
        }
        public (string json, RawParseResult? root) RawParse(string stmt)
        {
            using (ParsedStatementHandle handle = Pinshot_RustFFI.pinshot_blue_raw_parse(stmt))
            {
                var result = handle.AsString();
                using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(result)))
                {
                    // Deserialization from JSON
                    DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(RawParseResult));
                    var root = (RawParseResult?)deserializer.ReadObject(ms);
                    return (result, root);
                }
            }
        }
    }
}
