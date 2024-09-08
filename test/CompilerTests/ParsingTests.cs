using AnnaSim.Cpu;
using AnnaSim.Cpu.Memory;
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
    public void ParseComplexIf() => Parse("fixtures/complex_if.c");

    [TestMethod]
    public void ParseWhile() => Parse("fixtures/while.c");

    [TestMethod]
    public void ParseDoWhile() => Parse("fixtures/do-while.c");

    [TestMethod]
    public void ParseFor() => Parse("fixtures/for.c");
}