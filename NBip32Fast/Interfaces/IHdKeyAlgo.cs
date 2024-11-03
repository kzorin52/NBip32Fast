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
    public void GetPublic(ReadOnlySpan<byte> privateKey, Span<byte> publicKey);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static byte[] Bip32Hash(ReadOnlySpan<byte> chainCode, KeyPathElement index, ReadOnlySpan<byte> data)
    {
        var output = new byte[64];
        Bip32Hash(chainCode, index, data, output);

        return output;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static void Bip32Hash(ReadOnlySpan<byte> chainCode, KeyPathElement index, ReadOnlySpan<byte> data, Span<byte> output)
    {
        Span<byte> hmacAlloc = stackalloc byte[data.Length + 4];
        data.CopyTo(hmacAlloc.Slice(0, data.Length));
        index.Serialize(hmacAlloc.Slice(data.Length, 4));

        HMACSHA512.HashData(chainCode, hmacAlloc, output);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static byte[] Bip32SoftHash(ReadOnlySpan<byte> chainCode, KeyPathElement index, ReadOnlySpan<byte> privateKey, IHdKeyAlgo keyAlgo)
    {
        var output = new byte[64];
        Bip32SoftHash(chainCode, index, privateKey, keyAlgo, output);

        return output;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static void Bip32SoftHash(ReadOnlySpan<byte> chainCode, KeyPathElement index, ReadOnlySpan<byte> privateKey, IHdKeyAlgo keyAlgo, Span<byte> output)
    {
        Span<byte> hmacAlloc = stackalloc byte[33 + 4];
        keyAlgo.GetPublic(privateKey, hmacAlloc.Slice(0, 33));
        index.Serialize(hmacAlloc.Slice(33, 4));

        HMACSHA512.HashData(chainCode, hmacAlloc, output);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static byte[] Bip32Hash(ReadOnlySpan<byte> chainCode, KeyPathElement index, byte prefix, ReadOnlySpan<byte> data)
    {
        var output = new byte[64];
        Bip32Hash(chainCode, index, prefix, data, output);

        return output;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static void Bip32Hash(ReadOnlySpan<byte> chainCode,
        KeyPathElement index,
        byte prefix,
        ReadOnlySpan<byte> data,
        Span<byte> output)
    {
        Span<byte> hmacAlloc = stackalloc byte[1 + data.Length + 4];
        hmacAlloc[0] = prefix;
        data.CopyTo(hmacAlloc.Slice(1, data.Length));
        index.Serialize(hmacAlloc.Slice(1 + data.Length, 4));

        HMACSHA512.HashData(chainCode, hmacAlloc, output);
    }
}