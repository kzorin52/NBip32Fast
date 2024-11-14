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
       | NBip32FastKey |  32.49 us | 0.088 us | 0.083 us |  1.00 |    0.00 |     120 B |        1.00 |
       | NBitcoinKey   | 447.77 us | 1.211 us | 1.074 us | 13.78 |    0.05 |    9664 B |       80.53 |
       | NetezosKey    | 635.40 us | 1.744 us | 1.546 us | 19.55 |    0.07 | 3185946 B |   26,549.55 |
     */
}