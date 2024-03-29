using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using NBip32Fast;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class SerCacheTest
{
    private const uint TestCase = 80u;
    public SerCacheTest()
    {
        if (SerCache.HardCache.Length != 100 || SerCache.SoftCache.Length != 100)
        {
            throw new Exception();
        }
    }

    [Benchmark]
    public ReadOnlyMemory<byte> GetSerCache()
    {
        return SerCache.SoftCache.Span[(int)TestCase];
    }

    [Benchmark]
    public ReadOnlyMemory<byte> GetSer()
    {
        return KeyPathElement.SerializeUInt32(TestCase);
    }
    /*
     * | Method      | Mean     | Error     | StdDev    |
       |------------ |---------:|----------:|----------:|
       | GetSerCache | 2.304 ns | 0.0112 ns | 0.0094 ns |
       | GetSer      | 3.318 ns | 0.0890 ns | 0.0989 ns |
    lol what the actually fuck i am doing
    okay so 30% faster (?????????)
     */
}