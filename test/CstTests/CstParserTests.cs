using AnnaSim.AsmParsing;

namespace AnnaSim.Test.CstTests;

[TestClass]
public class CstParserTests
{
    [TestMethod]
    public void TestEmptySource()
    {
        var instructions = CstParser.ParseSource("");
        Assert.AreEqual(0, instructions.Count);
    }

    [TestMethod]
    public void TestSingleLine()
    {
        var instructions = CstParser.ParseSource("label: lwi r1 &dest # comment");
        Assert.AreEqual(1, instructions.Count);
    }

    [TestMethod]
    public void TestTriviaAtEndOfFile()
    {
        var source = """
        # start

            add r0 r1 r2

        # end
        """;

        var instructions = CstParser.ParseSource(source);

        Assert.AreEqual(1, instructions.Count);
        var i = instructions[0];
        Assert.AreEqual(2, i.LeadingTrivia.Count);
        Assert.AreEqual(2, i.TrailingTrivia.Count);
    }

    [TestMethod]
    public void TestSmallFile()
    {
        var instructions = CstParser.ParseFile("fixtures/cst_test_file1.asm");
        Assert.AreEqual(1, instructions.Count);
        Assert.AreEqual(4, instructions[0].LeadingTrivia.Count);
        Assert.AreEqual(2, instructions[0].TrailingTrivia.Count);

        var leadingTypeExpected = new List<Type> { typeof(HeaderComment), typeof(BlankLine), typeof(InlineComment), typeof(BlankLine) };
        var leadingTypeActual = instructions[0].LeadingTrivia.Select(t => t.GetType()).ToList();
        CollectionAssert.AreEqual(leadingTypeExpected, leadingTypeActual);
    }

    [TestMethod]
    public void TestLargerFile()
    {
        var instructions = CstParser.ParseFile("fixtures/cst_test_file2.asm");
        Assert.AreEqual(104, instructions.Count);
    }

    [TestMethod]
    public void TestMultiLabelInstruction()
    {
        var source = """
        label1:
        label2:     add     r0 r1 r2
        """;

        var instructions = CstParser.ParseSource(source);
        Assert.AreEqual(1, instructions.Count);
        Assert.AreEqual(2, instructions[0].Labels.Count);
    }
}