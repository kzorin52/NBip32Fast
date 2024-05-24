﻿using System.Security.Cryptography;
using NBip32Fast.Interfaces;

namespace NBip32Fast.Ed25519;

public class Ed25519HdKey : IHdKeyAlgo
{
    public static readonly IHdKeyAlgo Instance = new Ed25519HdKey();
    private static readonly byte[] CurveBytes = "ed25519 seed"u8.ToArray();

    private Ed25519HdKey()
    {
    }

    public HdKey GetMasterKeyFromSeed(ReadOnlySpan<byte> seed)
    {
        Span<byte> i = HMACSHA512.HashData(CurveBytes, seed);
        return new HdKey(i[..32], i[32..]);
    }

    public byte[] GetPublic(ReadOnlySpan<byte> privateKey)
    {
        Span<byte> output = stackalloc byte[32];

        Org.BouncyCastle.Math.EC.Rfc8032.Ed25519.GeneratePublicKey(privateKey, output);
        return output.ToArray();
    }
    
    public Span<byte> GetPublicSpan(ReadOnlySpan<byte> privateKey)
    {
        Span<byte> output = new byte[32];

        Org.BouncyCastle.Math.EC.Rfc8032.Ed25519.GeneratePublicKey(privateKey, output);
        return output;
    }

    public HdKey Derive(HdKey parent, KeyPathElement index)
    {
        if (!index.Hardened)
        {
            // TODO: Ed25519 soft derivation
            throw new ArgumentException("Ed25519 soft derivation not yet implemented.", nameof(index));
        }

        Span<byte> i = IHdKeyAlgo.Bip32Hash(parent.ChainCode, index, 0x0, parent.PrivateKey);
        return new HdKey(i[..32], i[32..]);
    }

    /* some my benchamrks:
       | Method          | Mean        | Error    | StdDev   |
       |---------------- |------------:|---------:|---------:|
       | NSecPub         |    24.89 us | 0.030 us | 0.026 us |
       | GeraltPub       |    19.39 us | 0.032 us | 0.029 us | << fastest
       | MonocypherPub   |    22.93 us | 0.094 us | 0.088 us |
       | ChaosNaClPub    |    49.54 us | 0.163 us | 0.152 us |
       | BouncyCastlePub |    19.46 us | 0.087 us | 0.082 us | << also fast, but managed.
       | TweetNaClPub    | 1,013.73 us | 5.763 us | 5.391 us |
     */
}