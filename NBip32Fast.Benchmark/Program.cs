using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using NBip32Fast;

//var test = new Secp256K1Tests();
//Console.WriteLine(Convert.ToHexStringLower(test.NBitcoinKey()));
//Console.WriteLine(Convert.ToHexStringLower(test.NBip39FastKey()));
//Console.WriteLine(Convert.ToHexStringLower(test.NetezosKey()));

BenchmarkRunner.Run<Secp256K1Tests>();

//var test2 = new Ed25519Tests();
//Console.WriteLine(Convert.ToHexStringLower(test2.P3HdKey()));
//Console.WriteLine(Convert.ToHexStringLower(test2.NBip32FastKey()));
//Console.WriteLine(Convert.ToHexStringLower(test2.NetezosKey()));

BenchmarkRunner.Run<Ed25519Tests>();
//BenchmarkRunner.Run<SerCacheTest>();

//var test3 = new Secp256R1Tests();
//Console.WriteLine(Convert.ToHexStringLower(test3.NBip39FastKey()));
//Console.WriteLine(Convert.ToHexStringLower(test3.NetezosKey()));
BenchmarkRunner.Run<Secp256R1Tests>();

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

public class SerCacheTest
{
    private const uint TestCase = 80u;
    public SerCacheTest()
    {
        if (SerCache.HardCache.Length != 100 || SerCache.SoftCache.Length != 100)
        {
            throw new Exception();
        }
    }

    [Benchmark]
    public ReadOnlyMemory<byte> GetSerCache()
    {
        return SerCache.SoftCache.Span[(int)TestCase];
    }

    [Benchmark]
    public ReadOnlyMemory<byte> GetSer()
    {
        return KeyPathElement.SerializeUInt32(TestCase);
    }
    /*
     * | Method      | Mean     | Error     | StdDev    |
       |------------ |---------:|----------:|----------:|
       | GetSerCache | 2.304 ns | 0.0112 ns | 0.0094 ns |
       | GetSer      | 3.318 ns | 0.0890 ns | 0.0989 ns |
    lol what the actually fuck i am doing
    okay so 30% faster (?????????)
     */
}