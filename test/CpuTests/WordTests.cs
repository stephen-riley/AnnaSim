using AnnaSim.Cpu.Memory;

namespace AnnaSim.Test.CpuTests;

[TestClass]
public class WordTests
{
    [TestMethod]
    public void TestBitwiseAnd()
    {
        var w = new Word(0xffff);
        var res = w & 0xaa00;
        Assert.AreEqual(0xaa00, res);
    }

    [TestMethod]
    public void TestBitwiseOr()
    {
        var w = new Word(0xff00);
        var res = w | 0x00ff;
        Assert.AreEqual(0xffff, res);
    }

    [TestMethod]
    public void TestBitwiseNot()
    {
        var w = new Word(0b1010101010101010);
        var res = ~w;
        Assert.AreEqual(0b0101010101010101, res);
    }

    [TestMethod]
    public void TestBitwiseXor()
    {
        var w = new Word(0xffff);
        var res = w ^ 0xaaaa;
        Assert.AreEqual(0x5555, res);
    }

    [TestMethod]
    [DataRow(0b1111_1111_1111_1111u, 0b1010_0101u, 0b1111_1111_1010_0101u)]
    [DataRow(0b1111_1111_0101_1010u, 0xffffu, 0b1111_1111_1111_1111u)]
    public void TestSetLowerUnsigned(uint orig, uint mask, uint expected)
    {
        var w = new Word(orig);

        var v = w.SetLower(mask);

        Assert.AreEqual((Word)expected, v);
    }

    [TestMethod]
    [DataRow(0b1111_1111_1111_1111u, 0b1010_0101, 0b1111_1111_1010_0101u)]
    [DataRow(0b1111_1111_0101_1010u, -1, 0b1111_1111_1111_1111u)]
    public void TestSetLowerSigned(uint orig, int mask, uint expected)
    {
        var w = new Word(orig);

        var v = w.SetLower(mask);

        Assert.AreEqual((Word)expected, v);
    }

    [TestMethod]
    [DataRow(0b1111_1111_1111_1111u, 0b1010_0101_0000_1001u, 0b1010_0101_1111_1111u)]
    [DataRow(0b1001_1001_0011_1100u, 0b1010_1010_1111_1111u, 0b1010_1010_0011_1100u)]
    public void TestSetUpperUnsigned(uint orig, uint mask, uint expected)
    {
        var w = new Word(orig);

        var v = w.SetUpper(mask);

        Assert.AreEqual((Word)expected, v);
    }

    [TestMethod]
    [DataRow(0b1111_1111_1111_1111u, 0b1010_0101_1100_0011, 0b1010_0101_1111_1111u)]
    [DataRow(0b1010_0101_0011_1100u, -1, 0b1111_1111_0011_1100u)]
    public void TestSetUpperSigned(uint orig, int mask, uint expected)
    {
        var w = new Word(orig);

        var v = w.SetUpper(mask);

        Assert.AreEqual((Word)expected, v);
    }
}