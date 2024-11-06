using GeneratorBtcOpenSsl;

namespace CSharpRandGen
{
    public class Generator
    {
        public List<byte[]> GenerateCombinations(int count)
        {
            var obj = new RandGenCSharp();
            obj.Init(count, null, false);
            obj.Run();
            return obj.PrivKey;
        }
    }
}
