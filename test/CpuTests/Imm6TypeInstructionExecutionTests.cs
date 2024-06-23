using AnnaSim.Cpu;
using AnnaSim.Instructions;
using AnnaSim.Cpu.Memory;

namespace AnnaSim.Test.CpuTests;

[TestClass]
public class Imm6TypeInstructionExecutionTests
{
    [TestMethod]
    [DataRow(23, 5, 28)]
    [DataRow(20, -5, 15)]
    [DataRow(20, -30, -10)]
    public void TestAddiInstruction(int op1, int imm6, int result)
    {
        var cpu = new AnnaMachine();
        cpu.Registers[2] = (ushort)op1;

        InstructionDefinition idef = I.Lookup["addi"];
        var instruction = idef.ToInstruction(1, 2, (short)imm6);
        idef.Execute(cpu, instruction);

        Assert.AreEqual((Word)(ushort)result, cpu.Registers[1]);
    }

    [TestMethod]
    [DataRow(3, 3, 24)]
    [DataRow(3, 0, 3)]
    [DataRow(3, -1, 1)]
    [DataRow(3, -2, 0)]
    [DataRow(64, -3, 8)]
    public void TestShfInstruction(int op1, int imm6, int result)
    {
        var cpu = new AnnaMachine();
        cpu.Registers[2] = (ushort)op1;

        InstructionDefinition idef = I.Lookup["shf"];
        var instruction = idef.ToInstruction(1, 2, (short)imm6);
        idef.Execute(cpu, instruction);

        Assert.AreEqual((Word)(ushort)result, cpu.Registers[1]);
    }

    [TestMethod]
    [DataRow(100, 5, 65535 - 100 - 5)]
    [DataRow(10, 0, 65525)]
    [DataRow(1, -2, 0)]
    [DataRow(65535, 1, 65535)]
    public void TestLwInstruction(int addrBase, int offset, int expected)
    {
        var cpu = new AnnaMachine();
        cpu.Memory.Initialize(0u, Enumerable.Range(0, cpu.Memory.Length).Reverse().Select(n => (Word)(uint)n).ToArray());
        cpu.Registers[2] = (uint)addrBase;

        InstructionDefinition idef = I.Lookup["lw"];
        var instruction = idef.ToInstruction(1, 2, (short)offset);
        idef.Execute(cpu, instruction);

        Assert.AreEqual(expected, cpu.Registers[1]);
    }

    [TestMethod]
    [DataRow(23, 100, 5)]
    public void TestSwInstruction(int value, int addrBase, int offset)
    {
        var cpu = new AnnaMachine();
        cpu.Registers[1] = (uint)value;
        cpu.Registers[2] = (uint)addrBase;

        InstructionDefinition idef = I.Lookup["shf"];
        var instruction = idef.ToInstruction(1, 2, (short)offset);
        idef.Execute(cpu, instruction);

        Assert.AreEqual(value, cpu.Memory[(uint)(addrBase + offset)]);
    }
}