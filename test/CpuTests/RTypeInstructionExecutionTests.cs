using AnnaSim.Cpu;
using AnnaSim.Cpu.Instructions;
using AnnaSim.Cpu.Memory;
using AnnaSim.Extensions;

namespace AnnaSim.Test.CpuTests;

[TestClass]
public class RTypeInstructionExecutionTests
{
    [TestMethod]
    public void TestAddition()
    {
        var cpu = new AnnaMachine();
        cpu.Registers[2] = 10;
        cpu.Registers[3] = 20;

        // add r1, r2, r3
        var instruction = Instruction.Add(1, 2, 3);
        cpu.ExecuteRType(instruction);
        Assert.AreEqual((Word)30, cpu.Registers[1]);
    }

    [TestMethod]
    public void TestSubtraction()
    {
        var cpu = new AnnaMachine();
        cpu.Registers[2] = 20;
        cpu.Registers[3] = 15;

        // add r1, r2, r3
        var instruction = Instruction.Sub(1, 2, 3);
        cpu.ExecuteRType(instruction);
        Assert.AreEqual((Word)5, cpu.Registers[1]);
    }

    [TestMethod]
    public void TestMultiplication()
    {
        var cpu = new AnnaMachine();
        cpu.Registers[2] = 0xffff;
        cpu.Registers[3] = 0xcafe;

        // add r1, r2, r3
        var instruction = Instruction.And(1, 2, 3);
        cpu.ExecuteRType(instruction);
        Assert.AreEqual((Word)0xcafe, cpu.Registers[1]);
    }

    [TestMethod]
    public void TestDivision()
    {
        var cpu = new AnnaMachine();
        cpu.Registers[2] = 0xff00;
        cpu.Registers[3] = 0x00ff;

        // add r1, r2, r3
        var instruction = Instruction.Or(1, 2, 3);
        cpu.ExecuteRType(instruction);
        Assert.AreEqual((Word)0xffff, cpu.Registers[1]);
    }

    [TestMethod]
    public void TestNegation()
    {
        var cpu = new AnnaMachine();
        cpu.Registers[2] = 0xaaaa;

        // add r1, r2, r3
        var instruction = Instruction.Not(1, 2, 0);
        cpu.ExecuteRType(instruction);
        Assert.AreEqual((Word)0x5555, cpu.Registers[1]);
    }

    [TestMethod]
    public void TestJumpAndLinkRegisterAsSubroutineCall()
    {
        // Assume PC @10, calling subroutine @20
        var cpu = new AnnaMachine { Pc = 10 };
        ushort rd = 1;
        ushort rs1 = 2;

        cpu.Registers[rd] = 20;

        var instruction = Instruction.Jalr(rd, rs1);
        var newPc = cpu.ExecuteRType(instruction);
        Assert.AreEqual(20u, newPc);
        Assert.AreEqual((Word)11, cpu.Registers[rs1]);
    }

    [TestMethod]
    public void TestJumpAndLinkRegisterAsJump()
    {
        // Assume PC @10, jumping @20
        var cpu = new AnnaMachine { Pc = 10 };
        ushort rd = 1;
        cpu.Registers[rd] = 20;

        var instruction = Instruction.Jalr(rd);
        var newPc = cpu.ExecuteRType(instruction);
        Assert.AreEqual(20u, newPc);
    }

    [TestMethod]
    public void TestGettingInputs()
    {
        var cpu = new AnnaMachine(10, 20, 30);
        var addr = 1u;
        cpu.ExecuteRType(Instruction.In((ushort)addr++));
        cpu.ExecuteRType(Instruction.In((ushort)addr++));
        cpu.ExecuteRType(Instruction.In((ushort)addr));

        Assert.AreEqual((Word)10u, cpu.Registers[1]);
        Assert.AreEqual((Word)20u, cpu.Registers[2]);
        Assert.AreEqual((Word)30u, cpu.Registers[3]);
    }

    [TestMethod]
    public void TestGettingTooManyInputs()
    {
        var cpu = new AnnaMachine(10);
        cpu.ExecuteRType(Instruction.In(1));

        Assert.AreEqual((Word)10u, cpu.Registers[1]);
        Assert.ThrowsException<NoInputRemainingException>((Action)(() =>
        {
            cpu.ExecuteRType(Instruction.In(1));
        }));
    }

    [TestMethod]
    public void TestOutputInstructions()
    {
        var queue = new Queue<Word>();

        var cpu = new AnnaMachine
        {
            OutputCallback = (w) => queue.Enqueue(w)
        };

        var expected = new Word[] { 10, 20, 30 };

        expected.ForEach((Action<Word>)(n =>
        {
            cpu.Registers[(uint)(n / 10)] = n;
            var instruction = Instruction.Out((ushort)(n / 10));
            cpu.ExecuteRType(instruction);
        }));

        Assert.IsTrue(Enumerable.SequenceEqual(expected, queue));
    }
}