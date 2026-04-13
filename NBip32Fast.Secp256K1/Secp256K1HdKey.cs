using System.Security.Cryptography;
using NBip32Fast.Interfaces;
using NBip32Fast.Utils;
using Temnij.Crypto;

namespace NBip32Fast.Secp256K1;

public class Secp256K1HdKey : IBip32Deriver
{
    public static readonly IBip32Deriver Instance = new Secp256K1HdKey();
    private static ReadOnlySpan<byte> CurveBytes => "Bitcoin seed"u8;

    private Secp256K1HdKey() { }

    public int PublicKeySize => 33;

    public void GetMasterKeyFromSeed(ReadOnlySpan<byte> seed, ref Bip32Key result)
    {
        var resultSpan = result.Span;
        HMACSHA512.HashData(CurveBytes, seed, resultSpan);

        while (true)
        {
            if (SecP256k1Native.VerifyPrivateKey(resultSpan[..32])) return;
            
// ReSharper disable once StackAllocInsideLoop because of extremely low probability of hash to be >N
// probablity is around 2^-128
#pragma warning disable CA2014
            Span<byte> retryData = stackalloc byte[33];
            
            // SLIP-0010 retry: HMAC(Curve, 0x01 || IR)
            retryData[0] = 0x01;
            resultSpan[32..].CopyTo(retryData[1..]);
            
            HMACSHA512.HashData(CurveBytes, retryData, resultSpan);
        }
    }

    public void Derive(ref readonly Bip32Key parent, KeyPathElement index, ref Bip32Key result)
    {
        var parentSpan = !parent.Span.Overlaps(result.Span)
            ? parent.Span
            : stackalloc byte[64];

        if (parent.Span.Overlaps(result.Span))
            parent.Span.CopyTo(parentSpan);

        var parentKey = parentSpan[..32];
        var parentCc  = parentSpan[32..];
        var resultSpan = result.Span;

        if (index.Hardened)
            Bip32Utils.Bip32Hash(parentCc, index, 0x00, parentKey, resultSpan);
        else
            Bip32Utils.Bip32SoftHash(parentCc, index, parentKey, this, resultSpan);

        var key = resultSpan[..32];
        var cc  = resultSpan[32..];

        while (true)
        {
            if (SecP256k1Native.VerifyPrivateKey(key)
                && SecP256k1Native.Tweak(key, parentKey, SecP256k1Native.KeyType.PrivateKey, SecP256k1Native.TweakMode.Add))
                return;

            Bip32Utils.Bip32Hash(parentCc, index, 0x01, cc, resultSpan);
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