using System.Security.Cryptography;
using NBip32Fast.Interfaces;
using Nethermind.Crypto;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Math;

namespace NBip32Fast.Algos;

public class Secp256K1HdKey : IHdKeyAlgo
{
    private static readonly ReadOnlyMemory<byte> CurveBytes = new("Bitcoin seed"u8.ToArray());
    private static readonly BigInteger N = SecNamedCurves.GetByName("secp256k1").N;

    public HdKey GetMasterKeyFromSeed(in ReadOnlySpan<byte> seed)
    {
        var seedCopy = seed.ToArray().AsSpan();

        while (true)
        {
            HMACSHA512.HashData(CurveBytes.Span, seedCopy, seedCopy); // hope its okay
            var key = seedCopy[..32];
            var parse256Ll = new BigInteger(1, key);

            if (parse256Ll.CompareTo(N) >= 0 || parse256Ll.CompareTo(BigInteger.Zero) == 0) continue;

            var cc = seedCopy[32..];
            return new HdKey(key, cc);
        }
    }

    public HdKey Derive(HdKey parent, KeyPathElement index)
    {
        var hash = index.Hardened
            ? IHdKeyAlgo.Bip32Hash(parent.ChainCode, index, 0x0, parent.Key).AsSpan()
            : IHdKeyAlgo.Bip32Hash(parent.ChainCode, index, GetPublic(parent.Key)).AsSpan();

        var key = hash[..32];
        var cc = hash[32..];

        while (true)
        {
            var parse256Ll = new BigInteger(1, key);
            var kPar = new BigInteger(1, parent.Key);
            var keyInt = parse256Ll.Add(kPar).Mod(N);

            if (parse256Ll.CompareTo(N) >= 0 || keyInt.CompareTo(BigInteger.Zero) == 0)
            {
                hash = IHdKeyAlgo.Bip32Hash(parent.ChainCode, index, 1, cc).AsSpan();
                key = hash[..32];
                cc = hash[32..];
                continue;
            }

            var keyBytes = keyInt.ToByteArrayUnsigned().AsSpan();

            return keyBytes.Length == 32
                ? new HdKey(keyBytes, cc)
                : new HdKey((byte[])[.. new byte[32 - keyBytes.Length], .. keyBytes], cc); // padding, maybe okay
        }
    }

    public byte[] GetPublic(in ReadOnlySpan<byte> privateKey)
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