using AnnaSim.Cpu;
using AnnaSim.Cpu.Instructions;
using AnnaSim.Cpu.Memory;

namespace AnnaSim.Test.CpuTests;

[TestClass]
public class Imm8TypeInstructionExecutionTests
{
    [TestMethod]
    [DataRow(0, 100, 100)]
    [DataRow(0, -10, 0 - 10 + 1 + 65536)]
    [DataRow(1, 100, 1)]
    [DataRow(-1, 100, 1)]
    public void TestBeqInstruction(int testValue, int offset, int expectedPc)
    {
        var cpu = new AnnaMachine();
        cpu.Registers[1] = (Word)(uint)testValue;

        var instruction = Instruction.Beq(1, offset);
        cpu.ExecuteImm8Type(instruction);

        Assert.AreEqual((uint)expectedPc, cpu.Pc);
    }

    [TestMethod]
    [DataRow(0, 100, 1)]
    [DataRow(0, -10, 1)]
    [DataRow(1, 100, 100)]
    [DataRow(-1, -10, 0 - 10 + 65536)]
    public void TestBneInstruction(int testValue, int offset, int expectedPc)
    {
        var cpu = new AnnaMachine();
        cpu.Registers[1] = (Word)(uint)testValue;

        var instruction = Instruction.Bne(1, offset);
        cpu.ExecuteImm8Type(instruction);

        Assert.AreEqual((uint)expectedPc, cpu.Pc);
    }

    [TestMethod]
    [DataRow(0, 100, 1)]
    [DataRow(0, -10, 1)]
    [DataRow(1, 100, 100)]
    [DataRow(-1, 100, 1)]
    public void TestBgtInstruction(int testValue, int offset, int expectedPc)
    {
        var cpu = new AnnaMachine();
        cpu.Registers[1] = (Word)(uint)testValue;

        var instruction = Instruction.Bgt(1, offset);
        cpu.ExecuteImm8Type(instruction);

        Assert.AreEqual((uint)expectedPc, cpu.Pc);
    }

    [TestMethod]
    [DataRow(0, 100, 100)]
    [DataRow(0, -10, 0 - 10 + 65536)]
    [DataRow(1, 100, 100)]
    [DataRow(-1, 100, 1)]
    public void TestBgeInstruction(int testValue, int offset, int expectedPc)
    {
        var cpu = new AnnaMachine();
        cpu.Registers[1] = (Word)(uint)testValue;

        var instruction = Instruction.Bge(1, offset);
        cpu.ExecuteImm8Type(instruction);

        Assert.AreEqual((uint)expectedPc, cpu.Pc);
    }

    [TestMethod]
    [DataRow(0, 100, 1)]
    [DataRow(0, -10, 1)]
    [DataRow(1, 100, 1)]
    [DataRow(-1, 100, 101)]
    public void TestBltInstruction(int testValue, int offset, int expectedPc)
    {
        var cpu = new AnnaMachine();
        cpu.Registers[1] = (Word)(uint)testValue;

        var instruction = Instruction.Blt(1, offset);
        cpu.ExecuteImm8Type(instruction);

        Assert.AreEqual((uint)expectedPc, cpu.Pc);
    }

    [TestMethod]
    [DataRow(0, 100, 100)]
    [DataRow(0, -10, 0 - 10 + 1 + 65536)]
    [DataRow(1, 100, 1)]
    [DataRow(-1, -10, 0 - 10 + 65536)]
    public void TestBleInstruction(int testValue, int offset, int expectedPc)
    {
        var cpu = new AnnaMachine();
        cpu.Registers[1] = (Word)(uint)testValue;

        var instruction = Instruction.Ble(1, offset);
        cpu.ExecuteImm8Type(instruction);

        Assert.AreEqual((uint)expectedPc, cpu.Pc);
    }
}