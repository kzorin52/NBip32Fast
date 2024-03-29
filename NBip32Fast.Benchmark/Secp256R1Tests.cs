using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

[MemoryDiagnoser]
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

    [Benchmark]
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
       
       BenchmarkDotNet v0.13.12, Windows 11 (10.0.26020.1000)
       Intel Core i9-14900K, 1 CPU, 32 logical and 24 physical cores
       .NET SDK 9.0.100-preview.2.24119.3
         [Host]     : .NET 9.0.0 (9.0.24.11501), X64 RyuJIT AVX2
         DefaultJob : .NET 9.0.0 (9.0.24.11501), X64 RyuJIT AVX2
       
       
       | Method        | Mean       | Error   | StdDev  |
       |-------------- |-----------:|--------:|--------:|
       | NBip39FastKey |   163.8 us | 0.26 us | 0.32 us |
       | NetezosKey    | 1,447.5 us | 5.16 us | 4.57 us |
       
       // * Hints *
       Outliers
         Secp256R1Tests.NBip39FastKey: Default -> 7 outliers were removed (199.50 us..312.65 us)
         Secp256R1Tests.NetezosKey: Default    -> 1 outlier  was  removed, 2 outliers were detected (1.44 ms, 1.46 ms)
     */
}