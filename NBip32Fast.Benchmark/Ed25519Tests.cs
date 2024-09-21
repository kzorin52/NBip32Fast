using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

[MemoryDiagnoser(false)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class Ed25519Tests
{
    private const string KeyPath = "m/44'/888'/0'/0'/0'";
    private readonly ReadOnlyMemory<byte> _seedSpan;
    private readonly byte[] _seedBytes;

    public Ed25519Tests()
    {
        _seedBytes = RandomNumberGenerator.GetBytes(64);
        _seedSpan = new ReadOnlyMemory<byte>(_seedBytes);
    }

    [Benchmark]
    public byte[] P3HdKey()
    {
        return P3.Ed25519.HdKey.Ed25519HdKey.DerivePath(KeyPath, _seedSpan.Span).Key;
    }

    [Benchmark(Baseline = true)]
    public byte[] NBip32FastKey()
    {
        return NBip32Fast.Ed25519.Ed25519HdKey.Instance.DerivePath(KeyPath, _seedSpan.Span).PrivateKey.ToArray();
    }

    [Benchmark]
    public byte[] NetezosKey()
    {
        return Netezos.Keys.HDKey.FromSeed(_seedBytes, Netezos.Keys.ECKind.Ed25519).Derive(KeyPath).Key.GetBytes();
    }

    /*
       // * Summary *
       
       BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.5001)
       Intel Core i9-14900K, 1 CPU, 32 logical and 24 physical cores
       .NET SDK 9.0.100-preview.5.24258.8
         [Host]     : .NET 9.0.0 (9.0.24.25601), X64 RyuJIT AVX2
         DefaultJob : .NET 9.0.0 (9.0.24.25601), X64 RyuJIT AVX2
       
       
       | Method        | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
       |-------------- |---------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
       | NBip32FastKey | 4.272 us | 0.0255 us | 0.0226 us |  1.00 |    0.00 | 0.0839 |   1.59 KB |        1.00 |
       | NetezosKey    | 5.404 us | 0.0434 us | 0.0362 us |  1.26 |    0.01 | 0.3204 |   5.99 KB |        3.76 |
       | P3HdKey       | 5.939 us | 0.0783 us | 0.1045 us |  1.40 |    0.03 | 0.3433 |   6.33 KB |        3.97 |
       
       // * Hints *
       Outliers
         Ed25519Tests.NBip32FastKey: Default -> 1 outlier  was  removed (4.36 us)
         Ed25519Tests.NetezosKey: Default    -> 2 outliers were removed (5.53 us, 5.69 us)
         Ed25519Tests.P3HdKey: Default       -> 8 outliers were removed (9.89 us..10.27 us)
     */
}