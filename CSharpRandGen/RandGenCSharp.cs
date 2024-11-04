using System.Runtime.InteropServices;

namespace GeneratorBtcOpenSsl
{
    internal static class NativeMethods
    {
        public delegate void DlgCallback(IntPtr key, long size_key, long inx);

        [DllImport("RandGen.dll")]
        public static extern void Run(Int64 total_keys, IntPtr pathToFile, bool flushToConsole);

        [DllImport("RandGen.dll")]
        public static extern void Init();

        [DllImport("RandGen.dll")]
        public static extern void SetCallback(DlgCallback callback);
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
            PrivKey = new List<string>();
        }

        public List<string> PrivKey;

        public void fun(IntPtr key, long size_key, long inx)
        {
            var privkey = Marshal.PtrToStringAnsi(key);
            privkey = privkey.TrimStart(['0', 'x']);
            if (!string.IsNullOrEmpty(privkey))
            {
                PrivKey.Add(privkey);
            }
        }

        public void Run()
        {
            NativeMethods.Run(_total_keys, string.IsNullOrEmpty(_pathToFile) ? IntPtr.Zero : Marshal.StringToHGlobalAnsi(_pathToFile), _flushToConsole);
        }

        public void Init(Int64 total_keys, string? pathToFile, bool flushToConsole)
        {
            _total_keys = total_keys;
            _pathToFile = pathToFile;
            _flushToConsole = flushToConsole;

            if (!_isInit)
            {
                _isInit = true;
                NativeMethods.Init();
            }
            NativeMethods.SetCallback(callback);
        }

        public void Dispose()
        {
            PrivKey.Clear();
        }
    }
}