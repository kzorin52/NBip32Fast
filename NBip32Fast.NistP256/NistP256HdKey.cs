using System.Runtime.InteropServices;
using System.Security.Cryptography;
using NBip32Fast.Interfaces;
using Nethermind.Int256;

namespace NBip32Fast.NistP256;

public class NistP256HdKey : IHdKeyAlgo
{
    public static readonly IHdKeyAlgo Instance = new NistP256HdKey();
    private static readonly byte[] CurveBytes = "Nist256p1 seed"u8.ToArray();

    private static readonly UInt256 N =
        UInt256.Parse("115792089210356248762697446949407573529996955224135760342422259061068512044369");

    private NistP256HdKey()
    {
    }

    public HdKey GetMasterKeyFromSeed(ReadOnlySpan<byte> seed)
    {
        Span<byte> seedCopy = new byte[64];
        seed.CopyTo(seedCopy);

        while (true)
        {
            HMACSHA512.HashData(CurveBytes, seedCopy, seedCopy);

            var key = seedCopy[..32];
            var keyInt = new UInt256(key, true);
            if (keyInt > N || keyInt.IsZero) continue;

            return new HdKey(key, seedCopy[32..]);
        }
    }
    
    public void GetMasterKeyFromSeed(Span<byte> seed)
    {
        while (true)
        {
            HMACSHA512.HashData(CurveBytes, seed, seed);

            var keyInt = new UInt256(seed[..32], true);
            if (keyInt > N || keyInt.IsZero) continue;

            return;
        }
    }

    public HdKey Derive(HdKey parent, KeyPathElement index)
    {
        Span<byte> hash = index.Hardened
            ? IHdKeyAlgo.Bip32Hash(parent.ChainCode, index, 0x00, parent.PrivateKey)
            : IHdKeyAlgo.Bip32Hash(parent.ChainCode, index, GetPublic(parent.PrivateKey));

        var key = hash[..32];
        var cc = hash[32..];

        var parentKey = new UInt256(parent.PrivateKey, true);

        while (true)
        {
            key.Reverse();
            var keyInt = new UInt256(key);
            UInt256.AddMod(keyInt, parentKey, N, out var res);

            if (keyInt > N || res.IsZero)
            {
                IHdKeyAlgo.Bip32Hash(parent.ChainCode, index, 0x01, cc, hash);
                continue;
            }

            var keyBytes = res.ToBigEndian();
            return new HdKey(keyBytes, cc);
        }
    }

    public void Derive(HdKey parent, KeyPathElement index, Span<byte> result)
    {
        if (index.Hardened)
            IHdKeyAlgo.Bip32Hash(parent.ChainCode, index, 0x00, parent.PrivateKey, result);
        else
            IHdKeyAlgo.Bip32SoftHash(parent.ChainCode, index, parent.PrivateKey, this, result);

        var key = result[..32];
        var cc = result[32..];

        var parentKey = new UInt256(parent.PrivateKey, true);

        while (true)
        {
            key.Reverse();
            var keyInt = new UInt256(key);
            UInt256.AddMod(keyInt, parentKey, N, out var res);

            if (keyInt > N || res.IsZero)
            {
                IHdKeyAlgo.Bip32Hash(parent.ChainCode, index, 0x01, cc, result);
                continue;
            }

            res.ToBigEndian(result[..32]);
            return;
        }
    }

    public unsafe byte[] GetPublic(ReadOnlySpan<byte> privateKey)
    {
        var publicKey = new byte[33];

        fixed (byte* secKey = privateKey, pubKey = publicKey)
        {
            NistP256Net.NistP256.private_to_public_key(secKey, pubKey);
        }

        return publicKey;
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