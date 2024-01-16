using System.Security.Cryptography;

namespace NBip32Fast.Interfaces;

public interface IHdKeyAlgo
{
    public HdKey GetMasterKeyFromSeed(in ReadOnlySpan<byte> seed);
    public HdKey Derive(HdKey parent, KeyPathElement index);

    public HdKey DerivePath(in KeyPath path, in ReadOnlySpan<byte> seed)
    {
        return DerivePath(path.Elements, seed);
    }

    public HdKey DerivePath(in ReadOnlyMemory<KeyPathElement> path, in ReadOnlySpan<byte> seed)
    {
        var key = GetMasterKeyFromSeed(seed);
        return DeriveFromMasterKey(path, key);
    }

    public HdKey DeriveFromMasterKey(in KeyPath path, HdKey masterKey)
    {
        return DeriveFromMasterKey(path.Elements, masterKey);
    }

    public HdKey DeriveFromMasterKey(in ReadOnlyMemory<KeyPathElement> path, HdKey masterKey)
    {
        var result = masterKey;
        for (var i = 0; i < path.Length; i++) result = Derive(result, path.Span[i]);

        return result;
    }

    public byte[] GetPublic(in ReadOnlySpan<byte> privateKey);

    protected static byte[] Bip32Hash(in ReadOnlySpan<byte> chainCode, KeyPathElement index, byte[] data)
    {
        return HMACSHA512.HashData(chainCode, [.. data, .. index.Serialized.Span]);
    }

    protected static byte[] Bip32Hash(in ReadOnlySpan<byte> chainCode, KeyPathElement index, byte prefix,
        ReadOnlySpan<byte> data)
    {
        return HMACSHA512.HashData(chainCode, [prefix, .. data, .. index.Serialized.Span]);
    }
}