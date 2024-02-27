using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace NBip32Fast.Interfaces;

public interface IHdKeyAlgo
{
    public HdKey GetMasterKeyFromSeed(ReadOnlySpan<byte> seed);
    public HdKey Derive(HdKey parent, KeyPathElement index);

    public HdKey DerivePath(KeyPath path, ReadOnlySpan<byte> seed)
    {
        return DerivePath(path.Elements.Span, seed);
    }

    public HdKey DerivePath(ReadOnlySpan<KeyPathElement> path, ReadOnlySpan<byte> seed)
    {
        var key = GetMasterKeyFromSeed(seed);
        return DeriveFromMasterKey(path, key);
    }

    public HdKey DeriveFromMasterKey(KeyPath path, HdKey masterKey)
    {
        return DeriveFromMasterKey(path.Elements.Span, masterKey);
    }

    public HdKey DeriveFromMasterKey(ReadOnlySpan<KeyPathElement> path, HdKey masterKey)
    {
        var result = masterKey;
        foreach (var t in path)
            result = Derive(result, t);

        return result;
    }

    public byte[] GetPublic(ReadOnlySpan<byte> privateKey);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static byte[] Bip32Hash(ReadOnlySpan<byte> chainCode, KeyPathElement index, ReadOnlySpan<byte> data)
    {
        return HMACSHA512.HashData(chainCode, [.. data, .. index.Serialized.Span]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static byte[] Bip32Hash(ReadOnlySpan<byte> chainCode, KeyPathElement index, byte prefix, ReadOnlySpan<byte> data)
    {
        return HMACSHA512.HashData(chainCode, [prefix, .. data, .. index.Serialized.Span]);
    }
}