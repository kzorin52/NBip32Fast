using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using NBip32Fast;

//var test = new Secp256K1Tests();
//Console.WriteLine(Convert.ToHexStringLower(test.NBitcoinKey()));
//Console.WriteLine(Convert.ToHexStringLower(test.NBip39FastKey()));
//Console.WriteLine(Convert.ToHexStringLower(test.NetezosKey()));

//BenchmarkRunner.Run<Secp256K1Tests>();

//var test2 = new Ed25519Tests();
//Console.WriteLine(Convert.ToHexStringLower(test2.P3HdKey()));
//Console.WriteLine(Convert.ToHexStringLower(test2.NBip32FastKey()));
//Console.WriteLine(Convert.ToHexStringLower(test2.NetezosKey()));

//BenchmarkRunner.Run<Ed25519Tests>();
BenchmarkRunner.Run<SerCacheTest>();

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
        return NBip32Fast.Derivation.Secp256K1.DerivePath(KeyPath, _seedSpan.Span).Key.ToArray();
    }

    [Benchmark]
    public byte[] NetezosKey()
    {
        return Netezos.Keys.HDKey.FromSeed(_seedBytes, Netezos.Keys.ECKind.Secp256k1).Derive(KeyPath).Key.GetBytes();
    }

    /*
      // * Summary *
       
       BenchmarkDotNet v0.13.12, Windows 11 (10.0.26020.1000)
       11th Gen Intel Core i7-11700K 3.60GHz, 1 CPU, 16 logical and 8 physical cores
       .NET SDK 9.0.100-alpha.1.24060.1
         [Host]     : .NET 9.0.0 (9.0.24.5902), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
         DefaultJob : .NET 9.0.0 (9.0.24.5902), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
       
       
       | Method        | Mean      | Error    | StdDev   |
       |-------------- |----------:|---------:|---------:|
       | NBitcoinKey   | 699.54 us | 5.756 us | 4.494 us |
       | NBip39FastKey |  58.37 us | 0.626 us | 0.555 us |
       | NetezosKey    | 969.39 us | 9.176 us | 7.662 us |
       
       // * Hints *
       Outliers
         Secp256K1Tests.NBitcoinKey: Default   -> 3 outliers were removed (729.61 us..785.45 us)
         Secp256K1Tests.NBip39FastKey: Default -> 1 outlier  was  removed (63.85 us)
         Secp256K1Tests.NetezosKey: Default    -> 2 outliers were removed (998.58 us, 1.17 ms)
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
        return NBip32Fast.Derivation.Ed25519.DerivePath(KeyPath, _seedSpan.Span).Key.ToArray();
    }

    [Benchmark]
    public byte[] NetezosKey()
    {
        return Netezos.Keys.HDKey.FromSeed(_seedBytes, Netezos.Keys.ECKind.Ed25519).Derive(KeyPath).Key.GetBytes();
    }

    /*
      // * Summary *
       
       BenchmarkDotNet v0.13.12, Windows 11 (10.0.26020.1000)
       11th Gen Intel Core i7-11700K 3.60GHz, 1 CPU, 16 logical and 8 physical cores
       .NET SDK 9.0.100-alpha.1.24060.1
         [Host]     : .NET 9.0.0 (9.0.24.5902), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
         DefaultJob : .NET 9.0.0 (9.0.24.5902), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
       
       
       | Method                  | Mean     | Error     | StdDev    |
       |------------------------ |---------:|----------:|----------:|
       | P3HdKey                 | 9.932 us | 0.1545 us | 0.1290 us |
       | NBip32FastKey           | 7.126 us | 0.0319 us | 0.0266 us |
       | NetezosKey              | 9.242 us | 0.0867 us | 0.0677 us |
       
       // * Hints *
       Outliers
         Ed25519Tests.P3HdKey: Default       -> 2 outliers were removed (10.49 us, 12.07 us)
         Ed25519Tests.NBip32FastKey: Default -> 2 outliers were removed (7.31 us, 8.27 us)
         Ed25519Tests.NetezosKey: Default    -> 3 outliers were removed (9.69 us..11.23 us)
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