using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NBip32Fast.Tests;

[TestClass]
public class KeyPathTest
{
    private const string TestKeyPathParseDataStr = "m/44'/0'/0'/0/0";

    private static readonly KeyPath TestKeyPathParseData = new(
    [
        new KeyPathElement(44, true),
        KeyPathElement.ZeroHard,
        KeyPathElement.ZeroHard,
        KeyPathElement.ZeroSoft,
        KeyPathElement.ZeroSoft
    ]);

    [TestMethod]
    public void TestKeyPathParse()
    {
        Assert.AreEqual(TestKeyPathParseData, KeyPath.Parse(TestKeyPathParseDataStr));
    }

    [TestMethod]
    public void TestKeyPathToString()
    {
        Assert.AreEqual(TestKeyPathParseDataStr, TestKeyPathParseData.ToString());
    }
}