using System.Runtime.CompilerServices;
using NBip32Fast.Interfaces;

namespace NBip32Fast;

[InlineArray(MaxPublicKeySize)]
internal struct PublicKeyBuffer
{
    internal const int MaxPublicKeySize = 33;

    private byte _element0;
}

/// <summary>
/// Wraps a parent <see cref="Bip32Key"/> and lazily caches its serialized public key,
/// so deriving many soft children of the same parent costs one EC multiplication total
/// instead of one per child.
/// </summary>
public ref struct Bip32Parent
{
    private Bip32Key _key;
    private readonly IBip32Deriver _deriver;
    private PublicKeyBuffer _publicKey;
    private bool _hasPublicKey;

    public Bip32Parent(scoped ref readonly Bip32Key key, IBip32Deriver deriver)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(deriver.PublicKeySize, PublicKeyBuffer.MaxPublicKeySize, nameof(deriver));

        key.Span.CopyTo(_key.Span);
        _deriver = deriver;
    }

    public void Derive(KeyPathElement index, ref Bip32Key result)
    {
        if (index.Hardened)
        {
            _deriver.Derive(in _key, index, ref result);
            return;
        }

        Span<byte> publicKey = ((Span<byte>)_publicKey)[.._deriver.PublicKeySize];

        if (!_hasPublicKey)
        {
            _deriver.GetPublic(_key.Key, publicKey);
            _hasPublicKey = true;
        }

        _deriver.Derive(in _key, publicKey, index, ref result);
    }
}
