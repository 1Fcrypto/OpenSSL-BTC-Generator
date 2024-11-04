
using CSharpRandGen;
using HexGenerator;
using System.Diagnostics.Metrics;

namespace CheckerTest
{
    public class Tests
    {
        [Test]
        public void RandGenTest()
        {
            var privKey = new Generator().GenerateCombinations(1).FirstOrDefault();
            Assert.That(privKey.Length, Is.EqualTo(64), "Количества элементов не совпадает.");
        }        
        
        [Test]
        public void BytesToHexStringTest()
        {
            var byteArray = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };
            var expectedHexString = "deadbeef";
            var hexString = Run.BytesToHexString(byteArray);
            Assert.That(hexString, Is.EqualTo(expectedHexString), "Преобразование байтов в строку не соответствует ожидаемому результату.");
        }      
        
        [Test]
        public void HexStringToBytesTest()
        {
            var expectedByteArray = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };
            var byteArray = Run.HexStringToBytes("deadbeef");
            Assert.That(byteArray, Is.EqualTo(expectedByteArray), "Преобразование строку в байты не соответствует ожидаемому результату.");
        }

        [Test]
        public void AddressToHash160Test()
        {
            var base58Address = "1EHNa6Q4Jz2uvNExL497mE43ikXhwF6kZm";
            var expectedHash160 = "91b24bf9f5288532960ac687abb035127b1d28a5";
            var decoded = Run.Base58DecodeCheck(base58Address);
            var hash160 = new byte[20];
            Array.Copy(decoded, decoded.Length - 20, hash160, 0, 20);
            var resultAddressToHash160String = BitConverter.ToString(hash160).Replace("-", "").ToLower();
            Assert.That(resultAddressToHash160String, Is.EqualTo(expectedHash160), "Преобразование Base58 адреса в Hash160 не соответствует ожидаемому результату.");
        }

        [Test]
        public void BitConverterTest()
        {
            var byteArray = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };
            var expectedHexString = "deadbeef";
            var hexString = BitConverter.ToString(byteArray).Replace("-", "").ToLower();
            Assert.That(hexString, Is.EqualTo(expectedHexString), "Преобразование байтов в строку не соответствует ожидаемому результату.");
        }


        [Test]
        public void AddressToHash160BytesTest()
        {
            var base58Address = "1EHNa6Q4Jz2uvNExL497mE43ikXhwF6kZm";
            var hash160 = "91b24bf9f5288532960ac687abb035127b1d28a5";
            var expectedByteArrayHash160 = Run.HexStringToBytes(hash160);
            var byteArrayHash160 = Run.AddressToHash160Bytes(base58Address);
            Assert.That(byteArrayHash160, Is.EqualTo(expectedByteArrayHash160), "Преобразование в hash160 не соответствует ожидаемому результату.");
        }

    }
}
