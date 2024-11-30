using AnnaSim.TinyC;

namespace AnnaSim.Test.CompilerTests;

[TestClass]
public class ParsingTests
{
    private void ParseFile(string fname)
    {
        Compiler.TryCompile(File.ReadAllText(fname), out var asm, showParseTree: true);
        Assert.IsNotNull(asm);
    }

    private void Parse(string src)
    {
        Compiler.TryCompile(src, out var asm, showParseTree: true);
        Assert.IsNotNull(asm);
    }

    [TestMethod]
    public void TestParseComplexIf() => ParseFile("fixtures/complex_if.c");

    [TestMethod]
    public void TestParseWhile() => ParseFile("fixtures/while.c");

    [TestMethod]
    public void TestParseDoWhile() => ParseFile("fixtures/do-while.c");

    [TestMethod]
    public void TestParseFor() => ParseFile("fixtures/for.c");

    [TestMethod]
    public void TestUnaryMinusOfConstant() => Parse("int a = -3;");

    [TestMethod]
    public void TestUnaryMinusOfHexConstant() => Parse("int a = -0x10;");

    [TestMethod]
    public void TestUnaryMinusOfBinaryConstant() => Parse("int a = -0b11110000;");

    [TestMethod]
    public void TestUnaryMinusOfExpression() => ParseFile("fixtures/unary.c");

    [TestMethod]
    public void TestPostfixAssignments() => ParseFile("fixtures/postfix_operators.c");

    [TestMethod]
    public void TestOpEqualAssignments() => ParseFile("fixtures/op_equal.c");

    [TestMethod]
    public void TestForLoop() => ParseFile("fixtures/for.c");

    [TestMethod]
    public void TestForLoopAlt1() => ParseFile("fixtures/for_alt1.c");

    [TestMethod]
    public void TestForLoopAlt2() => ParseFile("fixtures/for_alt2.c");
}