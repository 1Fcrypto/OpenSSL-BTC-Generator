using System;

namespace Donate
{
    
    public class DonateAddress
    {
        private static string donateBTC = "bc1q6e56xh9hwhc64qvk80e5gm0sj6fqxnax722p93";
        private static string donateETH = "0x03648073CceC8e499413c9E9fF630336313dd01a";
        private static string donateLTC = "ltc1q76qc3dkges9xsa3wevrs5equw5arxa06tzpxh6";
        private static string donateXMR = "42UUoo24ht77huJQ3ant5QPMy2AfoKNMEJy2B18RdGtAFGuzRLdFFjyP3qoECTrLwkVcS53GTCzcXADR8GD9HKTMVCYktBU";
        private static string telegram = "t.me/cryptocoinsearch";

        public static void Show(string projectName)
        {
            Console.WriteLine("Donate BTC: {0}", donateBTC);
            Console.WriteLine("Donate ETH: {0}", donateETH);
            Console.WriteLine("Donate LTC: {0}", donateLTC);
            Console.WriteLine("Donate XMR: {0}", donateXMR);
            Console.WriteLine("Telegram: {0}", telegram);
            Console.WriteLine();
            Console.WriteLine(projectName);
            Console.WriteLine();
        }
    }
}
