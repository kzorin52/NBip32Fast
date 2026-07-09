using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NBip32Fast.Interfaces;

namespace NBip32Fast.Tests;

[TestClass]
public class Bip32ParentTest
{
    private static readonly KeyPath AccountPath = "m/44'/0'/0'/0";

    [TestMethod]
    public void CachedSoftDeriveMatchesPlainSecp256K1()
    {
        AssertCachedMatchesPlain(Secp256K1.Secp256K1HdKey.Instance);
    }

    [TestMethod]
    public void CachedSoftDeriveMatchesPlainNistP256()
    {
        AssertCachedMatchesPlain(NistP256.NistP256HdKey.Instance);
    }

    [TestMethod]
    public void CachedHardenedDeriveMatchesPlain()
    {
        var deriver = Secp256K1.Secp256K1HdKey.Instance;

        var parent = new Bip32Key();
        deriver.DerivePath(AccountPath.Elements, TestCase.Seed.Span, ref parent);

        var cachedParent = new Bip32Parent(in parent, deriver);

        var expected = new Bip32Key();
        var actual = new Bip32Key();

        deriver.Derive(in parent, KeyPathElement.Hard(7), ref expected);
        cachedParent.Derive(KeyPathElement.Hard(7), ref actual);

        Assert.IsTrue(actual.Span.SequenceEqual(expected.Span));
    }

    private static void AssertCachedMatchesPlain(IBip32Deriver deriver)
    {
        var parent = new Bip32Key();
        deriver.DerivePath(AccountPath.Elements, TestCase.Seed.Span, ref parent);

        var cachedParent = new Bip32Parent(in parent, deriver);

        for (var i = 0u; i < 16; i++)
        {
            var expected = new Bip32Key();
            var actual = new Bip32Key();

            deriver.Derive(in parent, KeyPathElement.Soft(i), ref expected);
            cachedParent.Derive(KeyPathElement.Soft(i), ref actual);

            Assert.IsTrue(actual.Span.SequenceEqual(expected.Span), $"Mismatch at child index {i}");
        }
    }
}
