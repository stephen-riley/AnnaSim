using AnnaSim.TinyC;

namespace AnnaSim.Test.CpuTests;

[TestClass]
public class ParsingTests
{
    private void Parse(string fname)
    {
        Compiler.TryCompile(fname, File.ReadAllText(fname), out var asm);
        Assert.IsNotNull(asm);
    }

    [TestMethod]
    public void TestParseComplexIf() => Parse("fixtures/complex_if.c");

    [TestMethod]
    public void TestParseWhile() => Parse("fixtures/while.c");

    [TestMethod]
    public void TestParseDoWhile() => Parse("fixtures/do-while.c");

    [TestMethod]
    public void TestParseFor() => Parse("fixtures/for.c");
}