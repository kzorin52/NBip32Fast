using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace NBip32Fast.Benchmark;

/// <summary>
/// Derives <see cref="Count"/> soft children of the same account key —
/// the typical "scan wallet addresses" workload.
/// </summary>
[MemoryDiagnoser(false)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class SiblingScanBenchmark
{
    private static readonly KeyPath AccountPath = "m/44'/60'/0'/0";

    [Params(100)]
    public int Count;

    private byte[] _seedBytes = null!;

    [GlobalSetup]
    public void Setup()
    {
        _seedBytes = RandomNumberGenerator.GetBytes(64);
    }

    [Benchmark(Baseline = true)]
    public byte PlainDerive()
    {
        var deriver = NBip32Fast.Secp256K1.Secp256K1HdKey.Instance;

        var parent = new Bip32Key();
        deriver.DerivePath(AccountPath.Elements, _seedBytes, ref parent);

        byte acc = 0;
        var child = new Bip32Key();
        for (var i = 0u; i < (uint)Count; i++)
        {
            deriver.Derive(in parent, KeyPathElement.Soft(i), ref child);
            acc ^= child.Key[0];
        }

        return acc;
    }

    [Benchmark]
    public byte CachedParentDerive()
    {
        var deriver = NBip32Fast.Secp256K1.Secp256K1HdKey.Instance;

        var parent = new Bip32Key();
        deriver.DerivePath(AccountPath.Elements, _seedBytes, ref parent);

        var cachedParent = new Bip32Parent(in parent, deriver);

        byte acc = 0;
        var child = new Bip32Key();
        for (var i = 0u; i < (uint)Count; i++)
        {
            cachedParent.Derive(KeyPathElement.Soft(i), ref child);
            acc ^= child.Key[0];
        }

        return acc;
    }
}
