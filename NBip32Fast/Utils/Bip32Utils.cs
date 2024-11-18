using NBip32Fast.Interfaces;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace NBip32Fast.Utils;

public static class Bip32Utils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Bip32Hash(ReadOnlySpan<byte> chainCode, ref readonly KeyPathElement index, ReadOnlySpan<byte> data, Span<byte> output)
    {
        Span<byte> hmacAlloc = stackalloc byte[data.Length + 4];
        data.CopyTo(hmacAlloc.Slice(0, data.Length));
        index.Serialize(hmacAlloc.Slice(data.Length, 4));

        HMACSHA512.HashData(chainCode, hmacAlloc, output);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Bip32SoftHash(ReadOnlySpan<byte> chainCode, ref readonly KeyPathElement index, ReadOnlySpan<byte> privateKey, IBip32Deriver keyAlgo, Span<byte> output)
    {
        var pubSize = keyAlgo.PublicKeySize;

        Span<byte> hmacAlloc = stackalloc byte[pubSize + 4];
        keyAlgo.GetPublic(privateKey, hmacAlloc.Slice(0, pubSize));
        index.Serialize(hmacAlloc.Slice(pubSize, 4));

        HMACSHA512.HashData(chainCode, hmacAlloc, output);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Bip32Hash(ReadOnlySpan<byte> chainCode,
        ref readonly KeyPathElement index,
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