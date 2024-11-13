using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NBip32Fast.Interfaces;

namespace NBip32Fast.Tests;

[TestClass]
public class DerivationTest
{
    private static readonly TestCase Case1Ed25519 = new(
        "m/0'/0'/0'",
        "2f68c270a1e22070613ead10f5a00d352a0048440cfeef65aa986e84a10520bc",
        "b8e0e79ad924175d0bfaf1a3c481d258e43af1ecca966e056e68e07d05fe7da5");

    private static readonly TestCase Case1SecP256K1 = new(
        "m/0'/0/0",
        "6144c1daf8222d6dab77e7a20c2f338519b83bd1423602c56c7dfb5e9ea99c02",
        "55b36970e7ab8434f9b04f1c2e52da7422d2bce7e284ca353419dddfa2e34bdb");

    private static readonly TestCase Case1NistP256 = new(
        "m/0'/0/0",
        "159dd0bcc8c6982fcbcf53d77a914d32cdae5ae170b5d6d14557f7c141d507f5",
        "a96fed72636cb6a63ceb2a2e3a8a8ca8cff872ef4f102ebfa5f28bcc8f823ff5");

    [TestMethod]
    public void TestBip32Ed25519()
    {
        var der1 = new Bip32Key();
        Ed25519.Ed25519HdKey.Instance.DerivePath(Case1Ed25519.Path, TestCase.Seed.Span, ref der1);

        Assert.IsTrue(der1.Key.SequenceEqual(Case1Ed25519.Key.Span));
        Assert.IsTrue(der1.ChainCode.SequenceEqual(Case1Ed25519.ChainCode.Span));
    }

    [TestMethod]
    public void TestBip32Secp256K1()
    {
        var der1 = new Bip32Key();
        Secp256K1.Secp256K1HdKey.Instance.DerivePath(Case1SecP256K1.Path, TestCase.Seed.Span, ref der1);

        Assert.IsTrue(der1.Key.SequenceEqual(Case1SecP256K1.Key.Span));
        Assert.IsTrue(der1.ChainCode.SequenceEqual(Case1SecP256K1.ChainCode.Span));
    }

    [TestMethod]
    public void TestBip32NistP256()
    {
        var der1 = new Bip32Key();
        NistP256.NistP256HdKey.Instance.DerivePath(Case1NistP256.Path, TestCase.Seed.Span, ref der1);

        Assert.IsTrue(der1.Key.SequenceEqual(Case1NistP256.Key.Span));
        Assert.IsTrue(der1.ChainCode.SequenceEqual(Case1NistP256.ChainCode.Span));
    }
}

internal readonly struct TestCase(in string path, in string keyHex, in string ccHex)
{
    internal static readonly ReadOnlyMemory<byte> Seed = Convert.FromHexString(
        "e4a964f4973ce5750a6a5a5126e8258442c197b2e71b683ccba58688f21242eae1b0f12bee21d6e983d4a5c61f081bf3f0669546eb576dec1b22ec8d481b00fb");

    internal readonly ReadOnlyMemory<byte> Key = Convert.FromHexString(keyHex);
    internal readonly ReadOnlyMemory<byte> ChainCode = Convert.FromHexString(ccHex);

    internal readonly KeyPath Path = path;
}