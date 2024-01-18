using System.Security.Cryptography;
using NBip32Fast.Interfaces;
using Nethermind.Crypto;
using Nethermind.Int256;

namespace NBip32Fast.Algos;

public class Secp256K1HdKey : IHdKeyAlgo
{
    private static readonly ReadOnlyMemory<byte> CurveBytes = new("Bitcoin seed"u8.ToArray());
    private static readonly UInt256 N = UInt256.Parse("115792089237316195423570985008687907852837564279074904382605163141518161494337");

    public HdKey GetMasterKeyFromSeed(ReadOnlySpan<byte> seed)
    {
        var seedCopy = seed.ToArray().AsSpan();

        while (true)
        {
            HMACSHA512.HashData(CurveBytes.Span, seedCopy, seedCopy); // hope its okay

            var key = seedCopy[..32];
            var keyInt = new UInt256(key, true);
            if (keyInt > N || keyInt.IsZero) continue;

            return new HdKey(key, seedCopy[32..]);
        }
    }

    public HdKey Derive(HdKey parent, KeyPathElement index)
    {
        var hash = index.Hardened
            ? IHdKeyAlgo.Bip32Hash(parent.ChainCode, index, 0x00, parent.Key).AsSpan()
            : IHdKeyAlgo.Bip32Hash(parent.ChainCode, index, GetPublic(parent.Key)).AsSpan();

        var key = hash[..32];
        var cc = hash[32..];

        var parentKey = new UInt256(parent.Key, true);

        while (true)
        {
            var keyInt = new UInt256(key, true);
            UInt256.AddMod(keyInt, parentKey, N, out var res);

            if (keyInt > N || res.IsZero)
            {
                hash = IHdKeyAlgo.Bip32Hash(parent.ChainCode, index, 0x01, cc).AsSpan();
                key = hash[..32];
                cc = hash[32..];
                continue;
            }

            var keyBytes = res.ToBigEndian();
            return new HdKey(keyBytes, cc);
        }
    }

    public byte[] GetPublic(ReadOnlySpan<byte> privateKey)
    {
        return SecP256k1.GetPublicKey(privateKey.ToArray(), true)!;
    }

    /* some my benchamrks:
       | Method               | Mean      | Error     | StdDev    |
       |--------------------- |----------:|----------:|----------:|
       | BouncyCastlePub      | 484.89 us |  7.248 us |  6.780 us |
       | NBitcoinSecp256K1Pub | 123.40 us |  0.765 us |  0.716 us |
       | NBitcoinPub          | 114.46 us |  0.973 us |  0.862 us |
       | NativePub            |  36.64 us |  0.432 us |  0.404 us |
       | EcdsaLibPub          |  80.81 us |  0.792 us |  0.702 us |
       | NethermindPub        |  25.18 us |  0.187 us |  0.166 us | <-- [fastest]
       | LibplanetPub         | 963.74 us | 13.782 us | 12.892 us | <-- [slowest]
    */
}