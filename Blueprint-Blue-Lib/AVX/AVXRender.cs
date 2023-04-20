namespace AVX.Render
{
    using Blueprint;
    using System;
    using FlatSharp;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class AVXRenderingHandle : SafeHandle
    {
        public AVXRenderingHandle() : base(IntPtr.Zero, true) { }

        public override bool IsInvalid
        {
            get { return this.handle == IntPtr.Zero; }
        }

        public string Rendering
        {
            get
            {
                int len = 0;
                while (Marshal.ReadByte(handle, len) != 0) { ++len; }
                byte[] data = new byte[len];
                Marshal.Copy(handle, data, 0, data.Length);
                return Encoding.UTF8.GetString(data);
            }
        }

        protected override bool ReleaseHandle()
        {
            if (!this.IsInvalid)
            {
                AVXRendering.DeleteRendering(handle);
            }
            return true;
        }
    }

    public class AVXRendering
    {
        [DllImport("avx_search.dll", EntryPoint = "avx_create_rendering")]
        internal static extern AVXRenderingHandle CreateRendering(byte[] request);
        [DllImport("avx_search.dll", EntryPoint = "avx_delete_rendering")]
        internal static extern void DeleteRendering(IntPtr memory);

        public AVXRendering()
        {
            ;
        }
        public string Render(XRender render)
        {
            int maxBytesNeeded = XRender.Serializer.GetMaxSize(render);
            byte[] bytes = new byte[maxBytesNeeded];
            int bytesWritten = XRender.Serializer.Write(bytes, render);

            using (AVXRenderingHandle handle = AVXRendering.CreateRendering(bytes))
            {
                var result = handle.Rendering;
                return result;
            }
        }
    }
}
