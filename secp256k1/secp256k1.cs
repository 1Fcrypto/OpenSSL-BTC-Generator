using System.Runtime.InteropServices;

namespace secp256k1
{
    public class secp256k1
    {
        private const string libFile = "ice_secp256k1.so";
        private const string DllPath = "ice_secp256k1.dll";
        private static bool isLinux = false;

        static void Initialization()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                isLinux = false;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                isLinux = true;
            }
            else
            {
                Console.WriteLine("[-] Unsupported Platform currently for dll method. Only [Windows and Linux] is working");
                Environment.Exit(1);
            }
        }

        private static class IceLibrary_linux
        {

            [DllImport(libFile)]
            public static extern void privatekey_to_h160(int addrType, bool isCompressed, string pvkInt, byte[] ret);

            [DllImport(libFile)]
            public static extern void init_secp256_lib();
        }
        private static class IceLibrary
        {
            [DllImport(DllPath)]
            public static extern void privatekey_to_h160(int addrType, bool isCompressed, string pvkInt, byte[] ret);

            [DllImport(DllPath)]
            public static extern void init_secp256_lib();
        }

        public byte[] PrivateKeyToH160(int addrType, bool isCompressed, string pvkInt)
        {
            if (isLinux)
            {
                byte[] h160 = new byte[20];
                IceLibrary_linux.privatekey_to_h160(addrType, isCompressed, pvkInt, h160);
                return h160;
            }
            else
            {
                byte[] h160 = new byte[20];
                IceLibrary.privatekey_to_h160(addrType, isCompressed, pvkInt, h160);
                return h160;

            }
        }

        public void InitSecp256Lib()
        {
            Initialization();
            if (isLinux)
            {
                IceLibrary_linux.init_secp256_lib();
            }
            else
            {
                IceLibrary.init_secp256_lib();

            }
        }

    }
}