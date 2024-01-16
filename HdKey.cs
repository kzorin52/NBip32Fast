using NBip32Fast.Algos;
using NBip32Fast.Interfaces;

namespace NBip32Fast;

public static class Derivation
{
    public static readonly IHdKeyAlgo Secp256K1 = new Secp256K1HdKey();
    public static readonly IHdKeyAlgo Ed25519 = new Ed25519HdKey();
}
public readonly ref struct HdKey(ReadOnlySpan<byte> key, ReadOnlySpan<byte> cc)
{
    public readonly ReadOnlySpan<byte> Key = key;
    public readonly ReadOnlySpan<byte> ChainCode = cc;
}