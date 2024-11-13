using BenchmarkDotNet.Attributes;

namespace NBip32Fast.Benchmark;

[MemoryDiagnoser]
public class ParseKeyPath
{
    private const string Path = "m/44'/888'/0'/0'/0/0/100";

    [Benchmark]
    public string NBitcoinParse()
    {
        return NBitcoin.KeyPath.Parse(Path).ToString();
    }

    [Benchmark(Baseline = true)]
    public string NBip32FastParse()
    {
        return NBip32Fast.KeyPath.Parse(Path).ToString();
    }

    [Benchmark]
    public string NetezosParse()
    {
        return Netezos.Keys.HDPath.Parse(Path).ToString();
    }
    /*
     * * Summary *
       
       BenchmarkDotNet v0.14.1-nightly.20241027.193, Windows 11 (10.0.26100.2033)
       Unknown processor
       .NET SDK 10.0.100-alpha.1.24558.5
         [Host]     : .NET 10.0.0 (10.0.24.55701), X64 RyuJIT AVX2
         DefaultJob : .NET 10.0.0 (10.0.24.55701), X64 RyuJIT AVX2
       
       
       | Method                 | Mean     | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
       |----------------------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
       | NBitcoinParse          | 247.6 ns | 3.63 ns | 3.22 ns |  2.40 |    0.03 | 0.0620 |    1168 B |        2.28 |
       | NBip32FastParse        | 103.2 ns | 0.84 ns | 0.74 ns |  1.00 |    0.01 | 0.0272 |     512 B |        1.00 |
       | NetezosPars            | 275.2 ns | 1.72 ns | 1.52 ns |  2.67 |    0.02 | 0.0625 |    1184 B |        2.31 |
     */
}