using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using NBip32Fast.Interfaces;
using NBip32Fast.Utils;

namespace NBip32Fast.Ed25519;

public class Ed25519HdKey : IBip32Deriver
{
    public static readonly IBip32Deriver Instance = new Ed25519HdKey();
    private static readonly byte[] CurveBytes = "ed25519 seed"u8.ToArray();

    private Ed25519HdKey()
    {
    }

    public int PublicKeySize => 32;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetMasterKeyFromSeed(ReadOnlySpan<byte> seed, ref Bip32Key result)
    {
        HMACSHA512.HashData(CurveBytes, seed, result.Span);
    }

    public void Derive(ref Bip32Key parent, KeyPathElement index, ref Bip32Key result)
    {
        if (!index.Hardened)
            throw new ArgumentException("Ed25519 soft derivation not yet implemented.", nameof(index));

        Bip32Utils.Bip32Hash(parent.ChainCode, index, 0x0, parent.Key, result.Span);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetPublic(ReadOnlySpan<byte> privateKey, Span<byte> publicKey)
    {
        Org.BouncyCastle.Math.EC.Rfc8032.Ed25519.GeneratePublicKey(privateKey, publicKey);
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