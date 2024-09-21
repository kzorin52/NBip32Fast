using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

[MemoryDiagnoser(false)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class Secp256R1Tests
{
    private const string KeyPath = "m/44'/888'/0'/0/0";
    private readonly ReadOnlyMemory<byte> _seedSpan;
    private readonly byte[] _seedBytes;

    public Secp256R1Tests()
    {
        _seedBytes = RandomNumberGenerator.GetBytes(64);
        _seedSpan = new ReadOnlyMemory<byte>(_seedBytes);
    }

    [Benchmark(Baseline = true)]
    public byte[] NBip39FastKey()
    {
        return NBip32Fast.NistP256.NistP256HdKey.Instance.DerivePath(KeyPath, _seedSpan.Span).PrivateKey.ToArray();
    }

    [Benchmark]
    public byte[] NetezosKey()
    {
        return Netezos.Keys.HDKey.FromSeed(_seedBytes, Netezos.Keys.ECKind.NistP256).Derive(KeyPath).Key.GetBytes();
    }

    /*
     // * Summary *
       
       BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.5001)
       Intel Core i9-14900K, 1 CPU, 32 logical and 24 physical cores
       .NET SDK 9.0.100-preview.5.24258.8
         [Host]     : .NET 9.0.0 (9.0.24.25601), X64 RyuJIT AVX2
         DefaultJob : .NET 9.0.0 (9.0.24.25601), X64 RyuJIT AVX2
       
       
       | Method        | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0     | Gen1   | Allocated | Alloc Ratio |
       |-------------- |-----------:|---------:|---------:|------:|--------:|---------:|-------:|----------:|------------:|
       | NBip39FastKey |   158.3 us |  1.02 us |  2.37 us |  1.00 |    0.00 |        - |      - |   1.93 KB |        1.00 |
       | NetezosKey    | 1,324.4 us | 25.79 us | 40.91 us |  8.37 |    0.29 | 373.0469 | 1.9531 | 6857.9 KB |    3,553.89 |
       
       // * Hints *
       Outliers
         Secp256R1Tests.NBip39FastKey: Default -> 7 outliers were removed (256.97 us..299.87 us)
     */
}