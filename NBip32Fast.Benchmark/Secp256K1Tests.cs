using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

[MemoryDiagnoser]
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

    [Benchmark]
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
       
       BenchmarkDotNet v0.13.12, Windows 11 (10.0.26020.1000)
       Intel Core i9-14900K, 1 CPU, 32 logical and 24 physical cores
       .NET SDK 9.0.100-preview.2.24119.3
         [Host]     : .NET 9.0.0 (9.0.24.11501), X64 RyuJIT AVX2
         DefaultJob : .NET 9.0.0 (9.0.24.11501), X64 RyuJIT AVX2
       
       
       | Method        | Mean      | Error    | StdDev   |
       |-------------- |----------:|---------:|---------:|
       | NBitcoinKey   | 454.49 us | 0.789 us | 0.700 us |
       | NBip39FastKey |  38.10 us | 0.329 us | 0.308 us |
       | NetezosKey    | 647.82 us | 4.799 us | 4.254 us |
       
       // * Hints *
       Outliers
         Secp256K1Tests.NBitcoinKey: Default -> 1 outlier  was  removed, 2 outliers were detected (452.71 us, 456.58 us)
         Secp256K1Tests.NetezosKey: Default  -> 1 outlier  was  removed (682.74 us)
     */
}