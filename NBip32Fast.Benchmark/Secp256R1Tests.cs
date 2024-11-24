using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using NBip32Fast;
using NBip32Fast.Interfaces;

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
    public byte[] NBip32FastKey()
    {
        var der = new Bip32Key();
        NBip32Fast.NistP256.NistP256HdKey.Instance.DerivePath(NBip32Fast.KeyPath.Parse(KeyPath).Elements, _seedSpan.Span, ref der);

        return der.Key.ToArray();
    }

    [Benchmark]
    public byte[] NetezosKey()
    {
        return Netezos.Keys.HDKey.FromSeed(_seedBytes, Netezos.Keys.ECKind.NistP256).Derive(KeyPath).Key.GetBytes();
    }

    /*
     // * Summary *

        BenchmarkDotNet v0.14.1-nightly.20241027.193, Windows 11 (10.0.26100.2033)
        Unknown processor
        .NET SDK 10.0.100-alpha.1.24558.5
          [Host]     : .NET 10.0.0 (10.0.24.55701), X64 RyuJIT AVX2
          DefaultJob : .NET 10.0.0 (10.0.24.55701), X64 RyuJIT AVX2


        | Method        | Mean       | Error    | StdDev   | Ratio | RatioSD | Allocated | Alloc Ratio |
        |-------------- |-----------:|---------:|---------:|------:|--------:|----------:|------------:|
        | NBip32FastKey |   167.2 us |  0.53 us |  0.49 us |  1.00 |    0.00 |     608 B |        1.00 |
        | NetezosKey    | 1,483.8 us | 11.86 us | 10.51 us |  8.87 |    0.07 | 7029510 B |   11,561.69 |
     */
}