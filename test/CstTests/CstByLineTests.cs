using AnnaSim.AsmParsing;
using AnnaSim.Instructions;

namespace AnnaSim.Test.CstTests;

[TestClass]
public class CstByLineTests
{
    [TestMethod]
    public void TestEmplyLine()
    {
        var line = "";
        var parser = new Parser();
        var res = parser.TryParseLine(line, out var component);
        Assert.IsTrue(res);
        Assert.IsInstanceOfType<BlankLine>(component);
    }

    [TestMethod]
    public void TestOnlyWhitespace()
    {
        var line = "  \t \t  ";
        var parser = new Parser();
        var res = parser.TryParseLine(line, out var component);
        Assert.IsTrue(res);
        Assert.IsInstanceOfType<BlankLine>(component);
    }

    [TestMethod]
    public void TestInlineComment()
    {
        var line = "  \t \t  # testing";
        var parser = new Parser();
        var res = parser.TryParseLine(line, out var component);
        Assert.IsTrue(res);
        Assert.IsInstanceOfType<InlineComment>(component);
        Assert.AreEqual("testing", (component as InlineComment)?.Comment);
    }

    [TestMethod]
    public void TestHeaderComment()
    {
        var line = "# testing";
        var parser = new Parser();
        var res = parser.TryParseLine(line, out var component);
        Assert.IsTrue(res);
        Assert.IsInstanceOfType<HeaderComment>(component);
        Assert.AreEqual("testing", (component as HeaderComment)?.Comment);
    }

    [TestMethod]
    [DataRow(".org 0x8000", InstrOpcode._Org, "0x8000", null, null)]
    [DataRow("add r0 r1 r2", InstrOpcode.Add, "r0", "r1", "r2")]
    [DataRow("lwi r2 &label", InstrOpcode.Lwi, "r2", "&label", null)]
    public void TestBasicInstruction(string input, InstrOpcode opcode, string? op1, string? op2, string? op3)
    {
        var parser = new Parser();
        var res = parser.TryParseLine(input, out var component);
        Assert.IsTrue(res);
        Assert.IsInstanceOfType<CstInstruction>(component);

        var ci = component as CstInstruction;
        Assert.AreEqual(op1, ci?.Operand1);
        Assert.AreEqual(op2, ci?.Operand2);
        Assert.AreEqual(op3, ci?.Operand3);
    }

    [TestMethod]
    [DataRow("add   r0 r1 r2", null, InstrOpcode.Add, "r0", "r1", "r2", null)]
    [DataRow("\t\tlwi   r2 &label", null, InstrOpcode.Lwi, "r2", "&label", null, null)]
    [DataRow("label:\t\tlwi   r2 &label", "label", InstrOpcode.Lwi, "r2", "&label", null, null)]
    [DataRow("\t\tlwi   r2 &label       # comment", null, InstrOpcode.Lwi, "r2", "&label", null, "comment")]
    [DataRow("label:    lwi   r2 &label # comment", "label", InstrOpcode.Lwi, "r2", "&label", null, "comment")]
    public void TestMessyInstruction(string input, string? label, InstrOpcode opcode, string? op1, string? op2, string? op3, string? comment)
    {
        var parser = new Parser();
        var res = parser.TryParseLine(input, out var component);
        Assert.IsTrue(res);
        Assert.IsInstanceOfType<CstInstruction>(component);

        var ci = component as CstInstruction;
        Assert.AreEqual(op1, ci?.Operand1);
        Assert.AreEqual(op2, ci?.Operand2);
        Assert.AreEqual(op3, ci?.Operand3);

        if (label != null)
        {
            Assert.AreEqual(label, ci?.Labels?[0]);
        }

        if (comment != null)
        {
            Assert.AreEqual(comment, ci?.Comment);
        }
    }
}