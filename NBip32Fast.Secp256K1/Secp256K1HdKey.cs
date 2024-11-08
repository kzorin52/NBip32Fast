﻿using System.Security.Cryptography;
using NBip32Fast.Interfaces;
using Nethermind.Crypto;
using Nethermind.Int256;

namespace NBip32Fast.Secp256K1;

public class Secp256K1HdKey : IHdKeyAlgo
{
    public static readonly IHdKeyAlgo Instance = new Secp256K1HdKey();
    private static readonly byte[] CurveBytes = "Bitcoin seed"u8.ToArray();

    private static readonly UInt256 N =
        UInt256.Parse("115792089237316195423570985008687907852837564279074904382605163141518161494337");

    private Secp256K1HdKey()
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

            return new HdKey(key, seedCopy[32..]/*, []*/);
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

    public byte[] GetPublic(ReadOnlySpan<byte> privateKey)
    {
        return SecP256k1.GetPublicKey(privateKey, true)!;
    }

    public void GetPublic(ReadOnlySpan<byte> privateKey, Span<byte> publicKey)
    {
        SecP256k1.GetPublicKey(privateKey, true, publicKey);
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