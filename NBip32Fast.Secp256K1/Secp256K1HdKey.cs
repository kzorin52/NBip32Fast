using System.Security.Cryptography;
using NBip32Fast.Interfaces;
using NBip32Fast.Utils;
using Temnij.Crypto;

namespace NBip32Fast.Secp256K1;

public class Secp256K1HdKey : IBip32Deriver
{
    public static readonly IBip32Deriver Instance = new Secp256K1HdKey();
    private static readonly byte[] CurveBytes = "Bitcoin seed"u8.ToArray();

    private Secp256K1HdKey()
    {
    }

    public int PublicKeySize => 33;

    public void GetMasterKeyFromSeed(ReadOnlySpan<byte> seed, ref Bip32Key result)
    {
        var seedCopy = result.Span;
        seed.CopyTo(seedCopy);
        var key = seedCopy[..32];

        while (true)
        {
            HMACSHA512.HashData(CurveBytes, seedCopy, seedCopy);
            if (SecP256k1Native.VerifyPrivateKey(key)) return;
        }
    }

    public void Derive(ref Bip32Key parent, KeyPathElement index, ref Bip32Key result)
    {
        var parentSpan = parent.Span != result.Span
            ? parent.Span
            : stackalloc byte[64];
        if (parent.Span == result.Span) parent.Span.CopyTo(parentSpan);

        var resultSpan = result.Span;

        if (index.Hardened)
            Bip32Utils.Bip32Hash(parent.ChainCode, index, 0x00, parent.Key, resultSpan);
        else
            Bip32Utils.Bip32SoftHash(parent.ChainCode, index, parent.Key, this, resultSpan);

        var key = resultSpan[..32];
        var cc = resultSpan[32..];

        while (true)
        {
            if (SecP256k1Native.VerifyPrivateKey(key)
                && SecP256k1Native.Tweak(key, parentSpan[..32], SecP256k1Native.KeyType.PrivateKey, SecP256k1Native.TweakMode.Add)) return;

            Bip32Utils.Bip32Hash(parentSpan[32..], index, 0x01, cc, resultSpan);
        }
    }

    public void GetPublic(ReadOnlySpan<byte> privateKey, Span<byte> publicKey)
    {
        SecP256k1.GetPublicKey(publicKey, privateKey, SecP256k1Native.ECType.Compressed);
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