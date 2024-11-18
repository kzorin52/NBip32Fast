using System.Security.Cryptography;
using NBip32Fast.Interfaces;
using NBip32Fast.Utils;
using Nethermind.Int256;

namespace NBip32Fast.NistP256;

public class NistP256HdKey : IBip32Deriver
{
    public static readonly IBip32Deriver Instance = new NistP256HdKey();
    private static readonly byte[] CurveBytes = "Nist256p1 seed"u8.ToArray();

    private static readonly UInt256 N =
        UInt256.Parse("115792089210356248762697446949407573529996955224135760342422259061068512044369");

    private NistP256HdKey()
    {
    }

    public int PublicKeySize => 33;

    public void GetMasterKeyFromSeed(ReadOnlySpan<byte> seed, ref Bip32Key result)
    {
        var resultSpan = result.Span;
        seed.CopyTo(resultSpan);

        while (true)
        {
            HMACSHA512.HashData(CurveBytes, resultSpan, resultSpan);

            var keyInt = new UInt256(resultSpan[..32], true);
            if (keyInt > N || keyInt.IsZero) continue;

            return;
        }
    }

    public void Derive(ref readonly Bip32Key parent, ref readonly KeyPathElement index, ref Bip32Key result)
    {
        var parentChainCode = parent.Span != result.Span
            ? parent.Span[32..]
            : stackalloc byte[32];

        if (parent.Span == result.Span) parent.ChainCode.CopyTo(parentChainCode);

        var parentKey = new UInt256(parent.Key, true);
        var resultSpan = result.Span;

        if (index.Hardened)
            Bip32Utils.Bip32Hash(parent.ChainCode, in index, 0x00, parent.Key, resultSpan);
        else
            Bip32Utils.Bip32SoftHash(parent.ChainCode, in index, parent.Key, this, resultSpan);

        var key = resultSpan[..32];
        var cc = resultSpan[32..];


        while (true)
        {
            key.Reverse();
            var keyInt = new UInt256(key);
            UInt256.AddMod(keyInt, parentKey, N, out var res);

            if (keyInt > N || res.IsZero)
            {
                Bip32Utils.Bip32Hash(parentChainCode, in index, 0x01, cc, resultSpan);
                continue;
            }

            res.ToBigEndian(resultSpan[..32]);
            return;
        }
    }

    public unsafe void GetPublic(ReadOnlySpan<byte> privateKey, Span<byte> publicKey)
    {
        fixed (byte* secKey = privateKey, pubKey = publicKey)
        {
            NistP256Net.NistP256.private_to_public_key(secKey, pubKey);
        }
    }

    /*
       | Method          | Mean       | Error    | StdDev   |
       |---------------- |-----------:|---------:|---------:|
       | BouncyCastlePub | 1,149.3 us | 12.76 us | 11.31 us |
       | NativePub       |   143.1 us |  0.97 us |  0.90 us | // on rust-crypto/p256
     */
}