using AnnaSim.TinyC.Optimizer;
using AnnaSim.TinyC.Scheduler.Instructions;

using static AnnaSim.TinyC.Scheduler.Instructions.InstrOpcode;

namespace AnnaSim.Test.CpuTests;

[TestClass]
public class OptimizerTests
{
    [TestMethod]
    public void TestNoOptimizationsRun()
    {
        var instructions = new List<ScheduledInstruction>()
        {
            new ScheduledInstruction(null, And, "r0", "r1", "r2"),
            new ScheduledInstruction(null, Or, "r0", "r1", "r2"),
            new ScheduledInstruction(null, Out, "r0", null, null),
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
        var instructions = new List<ScheduledInstruction>()
        {
            new ScheduledInstruction(null, Push, "r7", "r3"),
            new ScheduledInstruction(null, Pop, "r7", "r3"),
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
        var instructions = new List<ScheduledInstruction>()
        {
            new ScheduledInstruction(null, Push, "r7", "r3"),
            new ScheduledInstruction(null, Pop, "r7", "r2"),
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

    // TODO: back to back beq r0
    // TODO: jump to next instruction

    // TODO: quick constant math (eg. n-1) (in the emitter, but it's an optimization)
}