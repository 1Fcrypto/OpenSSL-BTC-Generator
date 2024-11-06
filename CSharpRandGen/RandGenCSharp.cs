using System.Runtime.InteropServices;

namespace GeneratorBtcOpenSsl
{
    internal static class NativeMethods
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void DlgCallback([In] IntPtr key, [In] long size_key, [In] long inx);

        [DllImport("RandGen.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void Run([In] Int64 total_keys, [In] IntPtr pathToFile, [In] bool flushToConsole);

        [DllImport("RandGen.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Init();

        [DllImport("RandGen.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetCallback([MarshalAs(UnmanagedType.FunctionPtr)] DlgCallback callback, [In] bool add);
    }

    public class RandGenCSharp : IDisposable
    {

        volatile bool _isInit = false;
        NativeMethods.DlgCallback callback;
        Int64 _total_keys;
        string? _pathToFile;
        bool _flushToConsole;

        public RandGenCSharp()
        {
            callback = new(fun);
            PrivKey = new List<byte[]>();
        }

        public List<byte[]> PrivKey;

        public void fun(IntPtr key, long size_key, long inx)
        {
            byte[] btKey = new byte[size_key];
            Marshal.Copy(key, btKey, (int)0, (int)size_key);
            PrivKey.Add(btKey);

            //var privkey = Marshal.PtrToStringAnsi(key);
            //privkey = privkey.TrimStart(['0', 'x']);
            //if (!string.IsNullOrEmpty(privkey))
            //{
            //    PrivKey.Add(privkey);
            //}
        }

        public void Run()
        {
            NativeMethods.Run(_total_keys, string.IsNullOrEmpty(_pathToFile) ? IntPtr.Zero : Marshal.StringToHGlobalAnsi(_pathToFile), _flushToConsole);
            NativeMethods.SetCallback(callback, false);
        }

        public void Init(Int64 total_keys, string? pathToFile, bool flushToConsole)
        {
            _total_keys = total_keys;
            _pathToFile = pathToFile;
            _flushToConsole = flushToConsole;

            lock (nameof(RandGenCSharp))
            {
                if (!_isInit)
                {
                    _isInit = true;
                    NativeMethods.Init();
                }
                NativeMethods.SetCallback(callback, true);
            }
        }

        public void Dispose()
        {
            PrivKey.Clear();
        }
    }
}