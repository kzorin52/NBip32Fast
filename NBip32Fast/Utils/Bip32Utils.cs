using NBip32Fast.Interfaces;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace NBip32Fast.Utils;

public static class Bip32Utils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] Bip32Hash(ReadOnlySpan<byte> chainCode, KeyPathElement index, ReadOnlySpan<byte> data)
    {
        var output = new byte[64];
        Bip32Hash(chainCode, index, data, output);

        return output;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Bip32Hash(ReadOnlySpan<byte> chainCode, KeyPathElement index, ReadOnlySpan<byte> data, Span<byte> output)
    {
        Span<byte> hmacAlloc = stackalloc byte[data.Length + 4];
        data.CopyTo(hmacAlloc.Slice(0, data.Length));
        index.Serialize(hmacAlloc.Slice(data.Length, 4));

        HMACSHA512.HashData(chainCode, hmacAlloc, output);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] Bip32SoftHash(ReadOnlySpan<byte> chainCode, KeyPathElement index, ReadOnlySpan<byte> privateKey, IBip32Deriver keyAlgo)
    {
        var output = new byte[64];
        Bip32SoftHash(chainCode, index, privateKey, keyAlgo, output);

        return output;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Bip32SoftHash(ReadOnlySpan<byte> chainCode, KeyPathElement index, ReadOnlySpan<byte> privateKey, IBip32Deriver keyAlgo, Span<byte> output)
    {
        Span<byte> hmacAlloc = stackalloc byte[33 + 4];
        keyAlgo.GetPublic(privateKey, hmacAlloc.Slice(0, 33));
        index.Serialize(hmacAlloc.Slice(33, 4));

        HMACSHA512.HashData(chainCode, hmacAlloc, output);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] Bip32Hash(ReadOnlySpan<byte> chainCode, KeyPathElement index, byte prefix, ReadOnlySpan<byte> data)
    {
        var output = new byte[64];
        Bip32Hash(chainCode, index, prefix, data, output);

        return output;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Bip32Hash(ReadOnlySpan<byte> chainCode,
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