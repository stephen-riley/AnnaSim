using AnnaSim.Cpu;
using AnnaSim.Instructions;
using AnnaSim.Cpu.Memory;
using AnnaSim.Assembler;

namespace AnnaSim.Test.CpuTests;

[TestClass]
public class Imm6TypeInstructionExecutionTests
{
    static Imm6TypeInstructionExecutionTests()
    {
        // This is required to run these tests individually.
        InstructionDefinition.SetAssembler(new AnnaAssembler());
    }

    [TestMethod]
    [DataRow(23, 5, 28)]
    [DataRow(20, -5, 15)]
    [DataRow(20, -30, -10)]
    public void TestAddiInstruction(int op1, int imm6, int result)
    {
        var cpu = new AnnaMachine();
        cpu.Registers[2] = (ushort)op1;

        InstructionDefinition idef = ISA.Lookup["addi"];
        var instruction = idef.ToInstruction(rd: 1, rs1: 2, imm6: (short)imm6);
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

        InstructionDefinition idef = ISA.Lookup["shf"];
        var instruction = idef.ToInstruction(rd: 1, rs1: 2, imm6: (short)imm6);
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

        InstructionDefinition idef = ISA.Lookup["lw"];
        var instruction = idef.ToInstruction(rd: 1, rs1: 2, imm6: (short)offset);
        idef.Execute(cpu, instruction);

        Assert.AreEqual(expected, cpu.Registers[1]);
    }

    [TestMethod]
    [DataRow(1, 5, 32)]
    [DataRow(32, -5, 1)]
    public void TestSwInstruction(int start, int bitShift, int expected)
    {
        var cpu = new AnnaMachine();
        cpu.Registers[2] = (uint)start;

        InstructionDefinition idef = ISA.Lookup["shf"];
        var instruction = idef.ToInstruction(rd: 1, rs1: 2, imm6: (short)bitShift);
        idef.Execute(cpu, instruction);

        Assert.AreEqual(expected, cpu.Registers[1]);
    }
}