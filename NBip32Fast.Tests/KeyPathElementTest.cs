using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NBip32Fast.Tests;

[TestClass]
public class KeyPathElementTest
{
    private static readonly KeyPathElement TestData1 = new(44u, true);
    private static readonly ReadOnlyMemory<byte> TestData1Serialized = Convert.FromHexString("8000002c");

    [TestMethod]
    public void TestKeyElement()
    {
        Assert.IsTrue(((ReadOnlySpan<byte>)TestData1).SequenceEqual(TestData1Serialized.Span), 
            $"{TestData1.Number} [h={TestData1.Hardened}] [ser={Convert.ToHexStringLower(((ReadOnlySpan<byte>)TestData1))}] != {Convert.ToHexStringLower(TestData1Serialized.Span)}]");
    }
}