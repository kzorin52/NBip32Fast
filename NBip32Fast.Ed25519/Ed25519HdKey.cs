using System.Security.Cryptography;
using NBip32Fast.Interfaces;

namespace NBip32Fast.Ed25519;

public class Ed25519HdKey : IHdKeyAlgo
{
    public static readonly IHdKeyAlgo Instance = new Ed25519HdKey();
    private static readonly ReadOnlyMemory<byte> CurveBytes = new("ed25519 seed"u8.ToArray());

    private Ed25519HdKey()
    {
    }

    public HdKey GetMasterKeyFromSeed(ReadOnlySpan<byte> seed)
    {
        var i = HMACSHA512.HashData(CurveBytes.Span, seed).AsSpan();
        return new HdKey(i[..32], i[32..]);
    }

    public byte[] GetPublic(ReadOnlySpan<byte> privateKey)
    {
        Span<byte> publicKey = stackalloc byte[32];
        Span<byte> extendedPrivateKey = stackalloc byte[64];

        Geralt.Ed25519.GenerateKeyPair(publicKey, extendedPrivateKey, privateKey);

        return publicKey.ToArray();
    }
    
    public ReadOnlyMemory<byte> GetPublicMemory(ReadOnlySpan<byte> privateKey)
    {
        Memory<byte> publicKey = new byte[32];

        Span<byte> extendedPrivateKey = stackalloc byte[64]; // [..privateKey[0..32], ..publicKey[0..32]], not needed
        Geralt.Ed25519.GenerateKeyPair(publicKey.Span, extendedPrivateKey, privateKey);

        return publicKey;
    }

    public HdKey Derive(HdKey parent, KeyPathElement index)
    {
        if (!index.Hardened)
        {
            // TODO: Ed25519 soft derivation
            throw new ArgumentException("Ed25519 soft derivation not yet implemented.", nameof(index));
        }

        var i = IHdKeyAlgo.Bip32Hash(parent.ChainCode, index, 0x0, parent.PrivateKey).AsSpan();
        return new HdKey(i[..32], i[32..]);
    }

    /* some my benchamrks:
       | Method       | Mean     | Error    | StdDev   |
       |------------- |---------:|---------:|---------:|
       | NSecPub      | 24.77 us | 0.056 us | 0.049 us |
       | GeraltPub    | 19.25 us | 0.028 us | 0.025 us | << fastest because of no secure memory handles
       | ChaosNaClPub | 48.54 us | 0.057 us | 0.053 us |
     */
}