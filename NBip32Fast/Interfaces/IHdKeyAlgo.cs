using System.Runtime.CompilerServices;

namespace NBip32Fast.Interfaces;

public interface IBip32Deriver
{
    protected int PublicKeySize { get; }

    public void GetPublic(ReadOnlySpan<byte> privateKey, Span<byte> publicKey);

    public void GetMasterKeyFromSeed(ReadOnlySpan<byte> seed, ref Bip32Key result);
    public void Derive(ref Bip32Key parent, KeyPathElement index, ref Bip32Key result);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[] GetPublic(ReadOnlySpan<byte> privateKey)
    {
        var alloc = new byte[PublicKeySize];

        GetPublic(privateKey, alloc);
        return alloc;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DeriveFromMasterKey(ReadOnlySpan<KeyPathElement> path, ref Bip32Key masterKey, ref Bip32Key result)
    {
        foreach (var t in path)
            Derive(ref masterKey, t, ref result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DerivePath(ReadOnlySpan<KeyPathElement> path, ReadOnlySpan<byte> seed, ref Bip32Key result)
    {
        GetMasterKeyFromSeed(seed, ref result);
        DeriveFromMasterKey(path, ref result, ref result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DerivePath(in KeyPath path, ReadOnlySpan<byte> seed, ref Bip32Key result)
    {
        DerivePath(path.Elements, seed, ref result);
    }
}