using AnnaSim.Cpu;
using AnnaSim.Instructions;
using AnnaSim.Cpu.Memory;
using AnnaSim.Extensions;
using AnnaSim.Exceptions;

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
        var idef = I.Lookup["add"];
        var instruction = idef.ToInstruction(1, 2, 3);
        idef.Execute(cpu, instruction);

        Assert.AreEqual((Word)30, cpu.Registers[1]);
    }

    [TestMethod]
    public void TestSubtraction()
    {
        var cpu = new AnnaMachine();
        cpu.Registers[2] = 20;
        cpu.Registers[3] = 15;

        // sub r1, r2, r3
        var idef = I.Lookup["sub"];
        var instruction = idef.ToInstruction(1, 2, 3);
        idef.Execute(cpu, instruction);

        Assert.AreEqual((Word)5, cpu.Registers[1]);
    }

    [TestMethod]
    public void TestMultiplication()
    {
        var cpu = new AnnaMachine();
        cpu.Registers[2] = 0xffff;
        cpu.Registers[3] = 0xcafe;

        // and r1, r2, r3
        var idef = I.Lookup["and"];
        var instruction = idef.ToInstruction(1, 2, 3);
        idef.Execute(cpu, instruction);

        Assert.AreEqual((Word)0xcafe, cpu.Registers[1]);
    }

    [TestMethod]
    public void TestDivision()
    {
        var cpu = new AnnaMachine();
        cpu.Registers[2] = 0xff00;
        cpu.Registers[3] = 0x00ff;

        // or r1, r2, r3
        var idef = I.Lookup["or"];
        var instruction = idef.ToInstruction(1, 2, 3);
        idef.Execute(cpu, instruction);

        Assert.AreEqual((Word)0xffff, cpu.Registers[1]);
    }

    [TestMethod]
    public void TestNegation()
    {
        var cpu = new AnnaMachine();
        cpu.Registers[2] = 0xaaaa;

        // not r1, r2
        var idef = I.Lookup["not"];
        var instruction = idef.ToInstruction(1, 2);
        idef.Execute(cpu, instruction);

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

        var idef = I.Lookup["jalr"];
        var instruction = idef.ToInstruction(rd, rs1);
        var newPc = idef.Execute(cpu, instruction);

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

        var idef = I.Lookup["jalr"];
        var instruction = idef.ToInstruction(rd);
        var newPc = idef.Execute(cpu, instruction);

        Assert.AreEqual(20u, newPc);
    }

    [TestMethod]
    public void TestGettingInputs()
    {
        var cpu = new AnnaMachine([10, 20, 30]).Reset();
        var addr = 1u;

        var idef = I.Lookup["in"];
        idef.Execute(cpu, idef.ToInstruction((ushort)addr++));
        idef.Execute(cpu, idef.ToInstruction((ushort)addr++));
        idef.Execute(cpu, idef.ToInstruction((ushort)addr));

        Assert.AreEqual((Word)10u, cpu.Registers[1]);
        Assert.AreEqual((Word)20u, cpu.Registers[2]);
        Assert.AreEqual((Word)30u, cpu.Registers[3]);
    }

    [TestMethod]
    public void TestGettingTooManyInputs()
    {
        var cpu = new AnnaMachine([10]).Reset();
        var idef = I.Lookup["in"];
        var instruction = idef.ToInstruction(1);

        idef.Execute(cpu, instruction);

        Assert.AreEqual((Word)10u, cpu.Registers[1]);
        Assert.ThrowsException<NoInputRemainingException>((Action)(() =>
        {
            idef.Execute(cpu, idef.ToInstruction(1));
        }));
    }

    [TestMethod]
    public void TestOutputInstructions()
    {
        var queue = new Queue<Word>();

        var cpu = new AnnaMachine
        {
            OutputCallback = queue.Enqueue
        };

        var expected = new Word[] { 10, 20, 30 };
        var idef = I.Lookup["out"];

        expected.ForEach(n =>
        {
            cpu.Registers[(uint)(n / 10)] = n;
            var instruction = idef.ToInstruction((ushort)(n / 10));
            idef.Execute(cpu, instruction);
        });

        Assert.IsTrue(Enumerable.SequenceEqual(expected, queue));
    }
}