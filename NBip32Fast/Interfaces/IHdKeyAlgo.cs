using System.Runtime.CompilerServices;

namespace NBip32Fast.Interfaces;

public interface IBip32Deriver
{
    public int PublicKeySize { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetPublic(ReadOnlySpan<byte> privateKey, Span<byte> publicKey);

    public void GetMasterKeyFromSeed(ReadOnlySpan<byte> seed, ref Bip32Key result);
    public void Derive(ref readonly Bip32Key parent, ref readonly KeyPathElement index, ref Bip32Key result);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[] GetPublic(ReadOnlySpan<byte> privateKey)
    {
        var alloc = new byte[PublicKeySize];

        GetPublic(privateKey, alloc);
        return alloc;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DeriveFromMasterKey(ReadOnlySpan<KeyPathElement> path, ref readonly Bip32Key masterKey, ref Bip32Key result)
    {
        Derive(in masterKey, in path[0], ref result);
        for (var i = 1; i < path.Length; i++)
        {
            Derive(in result, in path[i], ref result);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DerivePath(ReadOnlySpan<KeyPathElement> path, ReadOnlySpan<byte> seed, ref Bip32Key result)
    {
        GetMasterKeyFromSeed(seed, ref result);
        DeriveFromMasterKey(path, ref result, ref result);
    }
}