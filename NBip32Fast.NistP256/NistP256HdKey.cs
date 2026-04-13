using System.Security.Cryptography;
using NBip32Fast.Interfaces;
using NBip32Fast.Utils;
using Nethermind.Int256;

namespace NBip32Fast.NistP256;

public class NistP256HdKey : IBip32Deriver
{
    public static readonly IBip32Deriver Instance = new NistP256HdKey();
    private static ReadOnlySpan<byte> CurveBytes => "Nist256p1 seed"u8;

    private static readonly UInt256 N =
        UInt256.Parse("115792089210356248762697446949407573529996955224135760342422259061068512044369");

    private NistP256HdKey()
    {
    }

    public int PublicKeySize => 33;

    public void GetMasterKeyFromSeed(ReadOnlySpan<byte> seed, ref Bip32Key result)
    {
        var resultSpan = result.Span;

        // Первая итерация: hash именно seed, любой длины
        HMACSHA512.HashData(CurveBytes, seed, resultSpan);

        while (true)
        {
            var keyInt = new UInt256(resultSpan[..32], isBigEndian: true);
            if (keyInt < N && !keyInt.IsZero) return;
            
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
        var parentChainCode = !parent.Span.Overlaps(result.Span)
            ? parent.Span[32..]
            : stackalloc byte[32];

        if (parent.Span.Overlaps(result.Span))
            parent.ChainCode.CopyTo(parentChainCode);

        var parentKey = new UInt256(parent.Key, isBigEndian: true);
        var resultSpan = result.Span;

        if (index.Hardened)
            Bip32Utils.Bip32Hash(parent.ChainCode, index, 0x00, parent.Key, resultSpan);
        else
            Bip32Utils.Bip32SoftHash(parent.ChainCode, index, parent.Key, this, resultSpan);

        var key = resultSpan[..32];
        var cc  = resultSpan[32..];

        while (true)
        {
            var keyInt = new UInt256(key, isBigEndian: true);
            UInt256.AddMod(keyInt, parentKey, N, out var res);

            if (keyInt >= N || res.IsZero)
            {
                Bip32Utils.Bip32Hash(parentChainCode, index, 0x01, cc, resultSpan);
                continue;
            }

            res.ToBigEndian(key);
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