namespace AVX.Search
{
    using Blueprint;
    using System;
    using FlatSharp;
    using System.Runtime.InteropServices;

    internal class AVXSearchHandle : SafeHandle
    {
        public AVXSearchHandle() : base(IntPtr.Zero, true) { }

        public override bool IsInvalid
        {
            get { return this.handle == IntPtr.Zero; }
        }

        public XResults Results
        {
            get
            {
                int len = 0;
                while (Marshal.ReadByte(handle, len) != 0) { ++len; }
                byte[] data = new byte[len];
                Marshal.Copy(handle, data, 0, data.Length);
                return XResults.Serializer.Parse(data);
            }
        }

        protected override bool ReleaseHandle()
        {
            if (!this.IsInvalid)
            {
                AVXSearch.DeleteSearch(handle);
            }
            return true;
        }
    }

    public class AVXSearch
    {
        [DllImport("avx_search.dll", EntryPoint = "avx_create_search")]
        internal static extern AVXSearchHandle CreateSearch(byte[] request);
        [DllImport("avx_search.dll", EntryPoint = "avx_delete_search")]
        internal static extern void DeleteSearch(IntPtr memory);

        public AVXSearch()
        {
            ;
        }
        public XResults Find(XRequest request)
        {
            int maxBytesNeeded = XRequest.Serializer.GetMaxSize(request);
            byte[] bytes = new byte[maxBytesNeeded];
            int bytesWritten = XRequest.Serializer.Write(bytes, request);

            using (AVXSearchHandle handle = AVXSearch.CreateSearch(bytes))
            {
                var result = handle.Results;
                return result;
            }
        }
    }
}
