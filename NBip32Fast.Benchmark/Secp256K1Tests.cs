using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

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
    public byte[] NBip39FastKey()
    {
        return NBip32Fast.Secp256K1.Secp256K1HdKey.Instance.DerivePath(KeyPath, _seedSpan.Span).PrivateKey.ToArray();
    }

    [Benchmark]
    public byte[] NetezosKey()
    {
        return Netezos.Keys.HDKey.FromSeed(_seedBytes, Netezos.Keys.ECKind.Secp256k1).Derive(KeyPath).Key.GetBytes();
    }

    /*
     // * Summary *
       
       BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.5001)
       Intel Core i9-14900K, 1 CPU, 32 logical and 24 physical cores
       .NET SDK 9.0.100-preview.5.24258.8
         [Host]     : .NET 9.0.0 (9.0.24.25601), X64 RyuJIT AVX2
         DefaultJob : .NET 9.0.0 (9.0.24.25601), X64 RyuJIT AVX2
       
       
       | Method        | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0     | Gen1   | Allocated  | Alloc Ratio |
       |-------------- |----------:|---------:|---------:|------:|--------:|---------:|-------:|-----------:|------------:|
       | NBip39FastKey |  36.32 us | 0.332 us | 0.310 us |  1.00 |    0.00 |   0.0610 |      - |    2.04 KB |        1.00 |
       | NBitcoinKey   | 409.24 us | 3.232 us | 2.865 us | 11.26 |    0.09 |   0.4883 |      - |    9.47 KB |        4.64 |
       | NetezosKey    | 556.16 us | 7.795 us | 7.291 us | 15.31 |    0.22 | 166.9922 | 0.9766 | 3083.32 KB |    1,512.13 |
       
       // * Hints *
       Outliers
         Secp256K1Tests.NBitcoinKey: Default -> 1 outlier  was  removed (422.32 us)
     */
}