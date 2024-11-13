using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using NBip32Fast;
using NBip32Fast.Interfaces;

[MemoryDiagnoser(false)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class Secp256K1Tests
{
    private const string KeyPath = "m/44'/888'/0'/0/0";
    private readonly ReadOnlyMemory<byte> _seedSpan;
    private readonly byte[] _seedBytes;

    public Secp256K1Tests()
    {
        _seedBytes = RandomNumberGenerator.GetBytes(64);
        _seedSpan = new ReadOnlyMemory<byte>(_seedBytes);
    }

    [Benchmark]
    public byte[] NBitcoinKey()
    {
        return NBitcoin.ExtKey.CreateFromSeed(_seedSpan.Span).Derive(NBitcoin.KeyPath.Parse(KeyPath)).PrivateKey.ToBytes();
    }

    [Benchmark(Baseline = true)]
    public byte[] NBip32FastKey()
    {
        var der = new Bip32Key();
        NBip32Fast.Secp256K1.Secp256K1HdKey.Instance.DerivePath(KeyPath, _seedSpan.Span, ref der);

        return der.Key.ToArray();
    }

    [Benchmark]
    public byte[] NetezosKey()
    {
        return Netezos.Keys.HDKey.FromSeed(_seedBytes, Netezos.Keys.ECKind.Secp256k1).Derive(KeyPath).Key.GetBytes();
    }

    /*
     // * Summary *
       
       BenchmarkDotNet v0.14.1-nightly.20241027.193, Windows 11 (10.0.26100.2033)
       Unknown processor
       .NET SDK 10.0.100-alpha.1.24558.5
         [Host]     : .NET 10.0.0 (10.0.24.55701), X64 RyuJIT AVX2
         DefaultJob : .NET 10.0.0 (10.0.24.55701), X64 RyuJIT AVX2
       
       
       | Method        | Mean      | Error    | StdDev   | Ratio | RatioSD | Allocated | Alloc Ratio |
       |-------------- |----------:|---------:|---------:|------:|--------:|----------:|------------:|
       | NBip32FastKey |  33.93 us | 0.239 us | 0.224 us |  1.00 |    0.01 |     608 B |        1.00 |
       | NBitcoinKey   | 512.24 us | 1.668 us | 1.393 us | 15.10 |    0.10 |    9665 B |       15.90 |
       | NetezosKey    | 655.89 us | 2.671 us | 2.367 us | 19.33 |    0.14 | 3200386 B |    5,263.79 |
     */
}