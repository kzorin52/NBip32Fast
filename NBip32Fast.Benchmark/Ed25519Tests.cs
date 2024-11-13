using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using NBip32Fast;
using NBip32Fast.Interfaces;

[MemoryDiagnoser(false)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class Ed25519Tests
{
    private const string KeyPath = "m/44'/888'/0'/0'/0'";
    public readonly ReadOnlyMemory<byte> _seedSpan;
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
        var der = new Bip32Key();
        NBip32Fast.Ed25519.Ed25519HdKey.Instance.DerivePath(KeyPath, _seedSpan.Span, ref der);

        return der.Key.ToArray();
    }

    [Benchmark]
    public byte[] NetezosKey()
    {
        return Netezos.Keys.HDKey.FromSeed(_seedBytes, Netezos.Keys.ECKind.Ed25519).Derive(KeyPath).Key.GetBytes();
    }

    /*
       // * Summary *
       
       BenchmarkDotNet v0.14.1-nightly.20241027.193, Windows 11 (10.0.26100.2033)
       Unknown processor
       .NET SDK 10.0.100-alpha.1.24558.5
         [Host]     : .NET 10.0.0 (10.0.24.55701), X64 RyuJIT AVX2
         DefaultJob : .NET 10.0.0 (10.0.24.55701), X64 RyuJIT AVX2
       
       
       | Method        | Mean     | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
       |-------------- |---------:|----------:|----------:|------:|----------:|------------:|
       | NBip32FastKey | 4.527 us | 0.0217 us | 0.0193 us |  1.00 |     672 B |        1.00 |
       | NetezosKey    | 6.041 us | 0.0258 us | 0.0242 us |  1.33 |    6136 B |        9.13 |
       | P3HdKey       | 6.415 us | 0.0626 us | 0.0555 us |  1.42 |    6480 B |        9.64 |
     */
}