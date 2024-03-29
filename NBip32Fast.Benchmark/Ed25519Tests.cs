using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

[MemoryDiagnoser]
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

    [Benchmark]
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
       
       BenchmarkDotNet v0.13.12, Windows 11 (10.0.26020.1000)
       Intel Core i9-14900K, 1 CPU, 32 logical and 24 physical cores
       .NET SDK 9.0.100-preview.2.24119.3
         [Host]     : .NET 9.0.0 (9.0.24.11501), X64 RyuJIT AVX2
         DefaultJob : .NET 9.0.0 (9.0.24.11501), X64 RyuJIT AVX2
       
       
       | Method        | Mean     | Error     | StdDev    |
       |-------------- |---------:|----------:|----------:|
       | P3HdKey       | 6.264 us | 0.1115 us | 0.1489 us |
       | NBip32FastKey | 4.561 us | 0.0242 us | 0.0227 us |
       | NetezosKey    | 5.962 us | 0.0300 us | 0.0281 us |
       
       // * Hints *
       Outliers
         Ed25519Tests.P3HdKey: Default    -> 8 outliers were removed (9.70 us..10.61 us)
         Ed25519Tests.NetezosKey: Default -> 1 outlier  was  detected (5.90 us)
     */
}