using System.Security.Cryptography;
using NBip32Fast.Interfaces;
using NSec.Cryptography;

namespace NBip32Fast.Algos;

public class Ed25519HdKey : IHdKeyAlgo
{
    private static readonly ReadOnlyMemory<byte> CurveBytes = new("ed25519 seed"u8.ToArray());

    public HdKey GetMasterKeyFromSeed(ReadOnlySpan<byte> seed)
    {
        var i = HMACSHA512.HashData(CurveBytes.Span, seed).AsSpan();
        return new HdKey(i[..32], i[32..]);
    }

    public byte[] GetPublic(ReadOnlySpan<byte> privateKey)
    {
        using var key = Key.Import(SignatureAlgorithm.Ed25519, privateKey, KeyBlobFormat.RawPrivateKey);
        return key.PublicKey.Export(KeyBlobFormat.RawPublicKey);
    }
    /* some my benchamrks:
       | Method       | Mean     | Error    | StdDev   |
       |------------- |---------:|---------:|---------:|
       | NSecPub      | 34.89 us | 0.340 us | 0.302 us | <-- [fastest]
       | ChaosNaClPub | 90.81 us | 1.507 us | 1.258 us | <-- [slowest]
       | HellNaClPub  | 67.98 us | 1.311 us | 1.561 us |
     */

    public HdKey Derive(HdKey parent, KeyPathElement index)
    {
        var i = IHdKeyAlgo.Bip32Hash(parent.ChainCode, index, 0x0, parent.Key).AsSpan();
        return new HdKey(i[..32], i[32..]);
    }
}