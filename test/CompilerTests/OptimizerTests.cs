using AnnaSim.AsmParsing;
using AnnaSim.TinyC.Optimizer;

using static AnnaSim.Instructions.InstrOpcode;

namespace AnnaSim.Test.CompilerTests;

[TestClass]
public class OptimizerTests
{
    [TestMethod]
    public void TestNoOptimizationsRun()
    {
        var instructions = new List<CstInstruction>()
        {
            new(null, And, "r0", "r1", "r2"),
            new(null, Or, "r0", "r1", "r2"),
            new(null, Out, "r0", null, null),
        };

        var opt = new Opt(instructions);
        var count = opt.Run();
        Assert.AreEqual(0, count);
        Assert.AreEqual(instructions.Count, opt.Instructions.Count);
    }

    [TestMethod]
    public void TestPushPopSameRegister()
    {
        // these cancel each other out; should remove both
        var instructions = new List<CstInstruction>()
        {
            new(null, Push, "r7", "r3"),
            new(null, Pop, "r7", "r3"),
        };

        var opt = new Opt(instructions);
        var count = opt.Run();
        Assert.AreEqual(1, count);
        Assert.AreEqual(0, opt.Instructions.Count);
    }

    [TestMethod]
    public void TestPushPopDifferentRegister()
    {
        // these almost cancel each other out--just move r2 <- r3
        var instructions = new List<CstInstruction>()
        {
            new(null, Push, "r7", "r3"),
            new(null, Pop, "r7", "r2"),
        };

        var opt = new Opt(instructions);
        var count = opt.Run();
        Assert.AreEqual(1, count);
        Assert.AreEqual(1, opt.Instructions.Count);

        var (_, opcode, op1, op2, _) = opt.Instructions.Pop();
        Assert.AreEqual(Mov, opcode);
        Assert.AreEqual("r2", op1);
        Assert.AreEqual("r3", op2);
    }

    [TestMethod]
    public void TestBranchToNextInstructionByLabel()
    {
        // No instructions between the branch and the target.
        // Remove the first instruction.
        var instructions = new List<CstInstruction>()
        {
            new(null, Beq, "r0", "&somewhere"),
            new("somewhere", Beq, "r0", "&somewhere_else"),
        };

        var opt = new Opt(instructions);
        var count = opt.Run();
        Assert.AreEqual(1, count);
        Assert.AreEqual(1, opt.Instructions.Count);

        var (_, opcode, op1, op2, _) = opt.Instructions.Pop();
        Assert.AreEqual(Beq, opcode);
        Assert.AreEqual("r0", op1);
        Assert.AreEqual("&somewhere_else", op2);
    }

    [TestMethod]
    public void TestBranchToNextInstructionByOffset()
    {
        // No instructions between the branch and the target.
        // Remove the first instruction.
        var instructions = new List<CstInstruction>()
        {
            new(null, Beq, "r0", "1"),
            new("somwhere", Beq, "r0", "&somewhere_else"),
        };

        var opt = new Opt(instructions);
        var count = opt.Run();
        Assert.AreEqual(1, count);
        Assert.AreEqual(1, opt.Instructions.Count);

        var (_, opcode, op1, op2, _) = opt.Instructions.Pop();
        Assert.AreEqual(Beq, opcode);
        Assert.AreEqual("r0", op1);
        Assert.AreEqual("&somewhere_else", op2);
    }

    [TestMethod]
    public void TestBackToBackJumps()
    {
        // Back to back jumps (beq r0) where the second one
        //  has no label--second can be removed.
        var instructions = new List<CstInstruction>()
        {
            new(null, Beq, "r0", "&somewhere"),
            new(null, Beq, "r0", "&somewhere_else"),
        };

        var opt = new Opt(instructions);
        var count = opt.Run();
        Assert.AreEqual(1, count);
        Assert.AreEqual(1, opt.Instructions.Count);
        Assert.AreEqual("&somewhere", opt.Instructions.Peek().Operand2);
    }

    [TestMethod]
    public void TestBackToBackJumpsSecondHasLabel()
    {
        // Back to back jumps (beq r0) where the second one
        //  HAS a label--we have to keep it in case it
        //  is a jump target.  Even if it isn't a target
        //  by label, it might be a target by offset.
        // Maybe at some point we can fix that, but not
        //  today.
        var instructions = new List<CstInstruction>()
        {
            new("label1", Beq, "r0", "&somewhere"),
            new("label2", Beq, "r0", "&somewhere_else"),
        };

        var opt = new Opt(instructions);
        var count = opt.Run();
        Assert.AreEqual(0, count);
        Assert.AreEqual(2, opt.Instructions.Count);
    }
}