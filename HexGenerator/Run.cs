using CSharpRandGen;
using Donate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HexGenerator
{
    public class Run
    {
        public static readonly object outFileLock = new object();
        public static bool IsRunning = false;
        public static int countThread = 0;
        public static long TotalHex = 0;
        public static long TotalCheck = 0;
        public static long Wet = 0;
        public static HashSet<byte[]> _addressDb = new HashSet<byte[]>(new ByteArrayComparer());
        public static DateTime start = DateTime.Now;
        public static secp256k1.secp256k1 _secp256k1 = new secp256k1.secp256k1();
        public static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        public static string filePath;
        public static List<string> checkRes = new List<string>();
        private const string Base58Alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

        static void Main(string[] args)
        {
            DonateAddress.Show("Start OpenSSL BTC Generator");
            try
            {
                if (args.Length < 1)
                {
                    Console.WriteLine("Specify the path to the address database");
                    return;
                }

                filePath = args[0];

                _secp256k1.InitSecp256Lib();
                Console.WriteLine($"How many thread to use? Min 1, Max {Environment.ProcessorCount}");
                int processorCount = Convert.ToInt32(Console.ReadLine());

                Console.WriteLine($"Count per thread");
                countThread = Convert.ToInt32(Console.ReadLine());


                Console.WriteLine("Reading addresses from {0}", "base.txt");
                LoadDatabase("base.txt", ref _addressDb);
                Console.WriteLine("Loading database done with {0} addresses...", _addressDb.Count);

                IsRunning = true;

                Task.Run(() =>
                {
                    long lastTotalHex = 0;
                    long lastTotalCheck = 0;
                    while (IsRunning)
                    {
                        var diff = DateTime.Now.Subtract(start);
                        long currentTotalHex = Interlocked.Read(ref TotalHex);
                        long currentTotalCheck = Interlocked.Read(ref TotalCheck);
                        long hexKeysPerSecond = (currentTotalHex - lastTotalHex);
                        long checkKeysPerSecond = (currentTotalCheck - lastTotalCheck);
                        lastTotalHex = currentTotalHex;
                        lastTotalCheck = currentTotalCheck;

                        Console.Write("\rTot_Hex: {0:N0} | Tot_Addr: {1:N0} | HEX_GEN k/s: {3:N0} | CHECK_ADDR k/s: {4:N0} | T: {2}",
                            TotalHex, TotalCheck,
                            String.Format("{0}:{1}:{2}", diff.Hours, diff.Minutes, diff.Seconds),
                            hexKeysPerSecond, checkKeysPerSecond);
                        Thread.Sleep(1000);
                    }
                });

                while (true)
                {
                    JobStart(processorCount);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void JobStart(int processorCount)
        {
            var threads = new List<Thread>();
            {

                var checkRes = new Generator().GenerateCombinations(countThread);

                for (int i = 0; i < processorCount; i++)
                {
                    var thread = new Thread(() =>
                    {
                        ProcessHexList(checkRes);
                    });
                    threads.Add(thread);
                    thread.Start();
                }

                foreach (var thread in threads)
                {
                    thread.Join();
                }

                checkRes.Clear();
            }
        }

        public static void ProcessHexList(IEnumerable<string> hexList)
        {
            foreach (var hex in hexList)
            {
                Interlocked.Increment(ref TotalHex);
                var uncompressedHash160 = _secp256k1.PrivateKeyToH160(0, false, hex);
                var compressedHash160 = _secp256k1.PrivateKeyToH160(0, true, hex);
                var p2shHash160 = _secp256k1.PrivateKeyToH160(1, true, hex);

                Check(HasBalance(uncompressedHash160), uncompressedHash160, hex);
                Check(HasBalance(compressedHash160), compressedHash160, hex);
                Check(HasBalance(p2shHash160), p2shHash160, hex);
            }
        }

        public static void LoadDatabase(string filePath, ref HashSet<byte[]> baseAddr)
        {
            foreach (string readLine in File.ReadLines(filePath))
            {
                if (readLine.StartsWith("1") || readLine.StartsWith("3"))
                {
                    baseAddr.Add(AddressToHash160Bytes(readLine));
                }    

            }
        }

        public static byte[] AddressToHash160Bytes(string address)
        {
            var decoded = Base58DecodeCheck(address);
            var hash160 = new byte[20];
            Array.Copy(decoded, decoded.Length - 20, hash160, 0, 20);
            return hash160;
        }

        public static bool HasBalance(byte[] address) => _addressDb.Contains(address);

        public static void Check(bool flag, byte[] address, string privKey)
        {
            Interlocked.Increment(ref TotalCheck);
            if (flag)
            {
                var findAddress = BytesToHexString(address);

                var jsonObject = new
                {
                    FindAddress = findAddress,
                    privKey
                };

                string contents = System.Text.Json.JsonSerializer.Serialize(jsonObject);
                Console.WriteLine("\n" + contents);
                lock (outFileLock)
                {
                    using (StreamWriter writer = new StreamWriter("______FIND_____.txt", true))
                    {
                        writer.WriteLine(contents);
                    }
                    Interlocked.Increment(ref Wet);
                }
            }
        }

        public static string BytesToHexString(byte[] bytes)
        {
            var stringBuilder = new StringBuilder(bytes.Length * 2);
            foreach (var byteb in bytes)
            {
                stringBuilder.AppendFormat("{0:x2}", byteb);
            }
            return stringBuilder.ToString();
        }

        public static byte[] HexStringToBytes(string hex)
        {
            if (hex.Length % 2 != 0)
            {
                throw new ArgumentException("Hex string must have an even length.");
            }

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }
        public static byte[] Base58DecodeCheck(string base58)
        {
            byte[] decoded = Base58Decode(base58);

            if (decoded.Length < 4)
                throw new FormatException("Invalid Base58Check string");

            byte[] data = decoded.Take(decoded.Length - 4).ToArray();
            byte[] checksum = decoded.Skip(decoded.Length - 4).ToArray();

            byte[] hash = SHA256.Create().ComputeHash(SHA256.Create().ComputeHash(data));
            byte[] calculatedChecksum = hash.Take(4).ToArray();

            if (!checksum.SequenceEqual(calculatedChecksum))
                throw new FormatException("Invalid checksum");

            return data;
        }

        public static byte[] Base58Decode(string base58)
        {
            BigInteger intData = 0;
            foreach (char c in base58)
            {
                int digit = Base58Alphabet.IndexOf(c);
                if (digit < 0)
                    throw new FormatException($"Invalid Base58 character `{c}` at position {base58.IndexOf(c)}");

                intData = intData * 58 + digit;
            }
            var result = intData.ToByteArray().Reverse().ToArray();
            int leadingZeroCount = base58.TakeWhile(c => c == '1').Count();
            var leadingZeros = new byte[leadingZeroCount];

            return leadingZeros.Concat(result.SkipWhile(b => b == 0)).ToArray();
        }
    }
}
