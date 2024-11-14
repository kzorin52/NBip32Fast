using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace NBip32Fast.Benchmark;

[MemoryDiagnoser(false)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
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
     // * Summary *
       
       BenchmarkDotNet v0.14.1-nightly.20241027.193, Windows 11 (10.0.26100.2033)
       Unknown processor
       .NET SDK 10.0.100-alpha.1.24558.5
         [Host]     : .NET 10.0.0 (10.0.24.55701), X64 RyuJIT AVX2
         DefaultJob : .NET 10.0.0 (10.0.24.55701), X64 RyuJIT AVX2
       
       
       | Method          | Mean      | Error    | StdDev   | Ratio | RatioSD | Allocated | Alloc Ratio |
       |---------------- |----------:|---------:|---------:|------:|--------:|----------:|------------:|
       | NBip32FastParse |  73.83 ns | 0.227 ns | 0.212 ns |  1.00 |    0.00 |      80 B |        1.00 |
       | NBitcoinParse   | 243.23 ns | 1.687 ns | 1.317 ns |  3.29 |    0.02 |    1168 B |       14.60 |
       | NetezosParse    | 278.73 ns | 0.836 ns | 0.782 ns |  3.78 |    0.01 |    1184 B |       14.80 |
     */
}