﻿
namespace Pinshot.Blue
{
    using AVSearch.Interfaces;
    using Blueprint.Blue;
    using Pinshot.PEG;
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization.Json;
    using System.Text;

    public static class Pinshot_RustFFI
    {
        [DllImport("pinshot_blue.dll", EntryPoint = "assert_grammar_revision")]
        internal static extern UInt16 assert_grammar_revision(UInt16 revision);
        [DllImport("pinshot_blue.dll", EntryPoint = "get_library_revision")]
        public static extern UInt16 get_library_revision();
        [DllImport("pinshot_blue.dll", EntryPoint = "create_quelle_parse_raw")]
        internal static extern ParsedStatementHandle pinshot_blue_raw_parse(string stmt);
        [DllImport("pinshot_blue.dll", EntryPoint = "create_quelle_parse")]
        internal static extern ParsedStatementHandle pinshot_blue_parse(string stmt);
        [DllImport("pinshot_blue.dll", EntryPoint = "delete_quelle_parse")]
        internal static extern void pinshot_blue_free(IntPtr memory);

        private static UInt16 ExpectedVersion = 0x5127;
        public static (UInt32 expected, bool okay) LibraryVersion
        {
            get
            {
                UInt16 actual = get_library_revision();
                UInt16 version = assert_grammar_revision(Pinshot_RustFFI.ExpectedVersion);

                Console.WriteLine("Using Quelle Grammar version: " + Pinshot_RustFFI.VERSION);
                if (actual != Pinshot_RustFFI.ExpectedVersion)
                    Console.WriteLine("Using Quelle Grammar version: " + Pinshot_RustFFI.VERSION_EXPECTED);

                return (actual, (version != 0) && (actual == Pinshot_RustFFI.ExpectedVersion));
            }
        }
        private static string _VERSION = "UNKNOWN";
        public static string VERSION
        {
            get
            {
                if (Pinshot_RustFFI._VERSION == "UNKNOWN")
                {
                    UInt16 actual = get_library_revision();
                    Pinshot_RustFFI._VERSION = ((actual & 0xF000) >> 12).ToString() + "." + ((actual & 0x0F00) >> 8).ToString() + "." + (actual & 0x00FF).ToString("X");

                    if (actual != Pinshot_RustFFI.ExpectedVersion)
                    {
                        Pinshot_RustFFI._VERSION = "actual: " + Pinshot_RustFFI._VERSION + " // " + "expected: " + Pinshot_RustFFI.VERSION_EXPECTED;
                    }
                }
                return Pinshot_RustFFI._VERSION;
            }
        }
        public static string VERSION_EXPECTED
        {
            get
            {
                return ((ExpectedVersion & 0xF000) >> 12).ToString() + "." + ((ExpectedVersion & 0x0F00) >> 8).ToString() + "." + (ExpectedVersion & 0x00FF).ToString("X");
            }
        }
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
        private void TrimParse(Parsed parse)
        {
            parse.text = parse.text.Trim();
            TrimParses(parse.children);
        }
        private void TrimParses(Parsed[] parses)
        {
            foreach (Parsed parse in parses)
            {
                TrimParse(parse);
            }
        }
        public (string json, RootParse? root) Parse(string stmt)
        {
            using (ParsedStatementHandle handle = Pinshot_RustFFI.pinshot_blue_parse(stmt))
            {
                var result = handle.AsString();
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(result)))
                {
                    // Deserialization from JSON
                    DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(RootParse));
                    var root = (RootParse?)deserializer.ReadObject(ms);
                    if (string.IsNullOrEmpty(root.error))
                    {
                        TrimParses(root.result);
                    }
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
