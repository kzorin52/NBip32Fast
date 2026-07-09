using System.Runtime.CompilerServices;

namespace NBip32Fast.Interfaces;

public interface IBip32Deriver
{
    public int PublicKeySize { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetPublic(ReadOnlySpan<byte> privateKey, Span<byte> publicKey);

    public void GetMasterKeyFromSeed(ReadOnlySpan<byte> seed, ref Bip32Key result);
    public void Derive(ref readonly Bip32Key parent, KeyPathElement index, ref Bip32Key result);

    /// <summary>
    /// Derive using a precomputed serialized parent public key — the soft-derivation
    /// fast path when deriving many siblings of the same parent (see <see cref="Bip32Parent"/>).
    /// The default implementation ignores the hint.
    /// </summary>
    public void Derive(ref readonly Bip32Key parent, scoped ReadOnlySpan<byte> parentPublicKey, KeyPathElement index, ref Bip32Key result)
    {
        Derive(in parent, index, ref result);
    }

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
        Derive(in masterKey, path[0], ref result);
        for (var i = 1; i < path.Length; i++) Derive(in result, path[i], ref result);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DeriveFromMasterKey(KeyPath path, ref readonly Bip32Key masterKey, ref Bip32Key result)
    {
        DeriveFromMasterKey(path.Elements, in masterKey, ref result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DerivePath(ReadOnlySpan<KeyPathElement> path, ReadOnlySpan<byte> seed, ref Bip32Key result)
    {
        GetMasterKeyFromSeed(seed, ref result);
        DeriveFromMasterKey(path, ref result, ref result);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DerivePath(KeyPath path, ReadOnlySpan<byte> seed, ref Bip32Key result)
    {
        DerivePath(path.Elements, seed, ref result);
    }
}