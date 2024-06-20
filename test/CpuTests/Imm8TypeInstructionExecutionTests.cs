using AnnaSim.Cpu;
using AnnaSim.Instructions;
using AnnaSim.Cpu.Memory;

namespace AnnaSim.Test.CpuTests;

[TestClass]
public class Imm8TypeInstructionExecutionTests
{
    [TestMethod]
    [DataRow(0, 100, 101)]
    // target is instruction's address (0) + 1 (PC+1 per instruction), + 65536 (normalize negative address)
    [DataRow(0, -10, 0 + 1 - 10 + 65536)]
    [DataRow(1, 100, 1)]
    [DataRow(-1, 100, 1)]
    public void TestBeqInstruction(int testValue, int offset, int expectedPc)
    {
        var cpu = new AnnaMachine();
        cpu.Registers[1] = (Word)(uint)testValue;

        var instruction = Instruction.Beq(1, offset);
        var newPc = cpu.ExecuteImm8Type(instruction);

        Assert.AreEqual((uint)expectedPc, newPc);
    }

    [TestMethod]
    [DataRow(0, 100, 1)]
    [DataRow(0, -10, 1)]
    [DataRow(1, 100, 101)]
    // target is instruction's address (0) + 1 (PC+1 per instruction), + 65536 (normalize negative address)
    [DataRow(-1, -10, 0 + 1 - 10 + 65536)]
    public void TestBneInstruction(int testValue, int offset, int expectedPc)
    {
        var cpu = new AnnaMachine();
        cpu.Registers[1] = (Word)(uint)testValue;

        var instruction = Instruction.Bne(1, offset);
        var newPc = cpu.ExecuteImm8Type(instruction);

        Assert.AreEqual((uint)expectedPc, newPc);
    }

    [TestMethod]
    [DataRow(0, 100, 1)]
    [DataRow(0, -10, 1)]
    [DataRow(1, 100, 101)]
    [DataRow(-1, 100, 1)]
    public void TestBgtInstruction(int testValue, int offset, int expectedPc)
    {
        var cpu = new AnnaMachine();
        cpu.Registers[1] = (Word)(uint)testValue;

        var instruction = Instruction.Bgt(1, offset);
        var newPc = cpu.ExecuteImm8Type(instruction);

        Assert.AreEqual((uint)expectedPc, newPc);
    }

    [TestMethod]
    [DataRow(0, 100, 101)]
    [DataRow(-1, -10, 1)]
    // target is instruction's address (0) + 1 (PC+1 per instruction), + 65536 (normalize negative address)
    [DataRow(0, -10, 0 + 1 - 10 + 65536)]
    [DataRow(1, 100, 101)]
    [DataRow(-1, 100, 1)]
    public void TestBgeInstruction(int testValue, int offset, int expectedPc)
    {
        var cpu = new AnnaMachine();
        cpu.Registers[1] = (Word)(uint)testValue;

        var instruction = Instruction.Bge(1, offset);
        var newPc = cpu.ExecuteImm8Type(instruction);

        Assert.AreEqual((uint)expectedPc, newPc);
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
        var newPc = cpu.ExecuteImm8Type(instruction);

        Assert.AreEqual((uint)expectedPc, newPc);
    }

    [TestMethod]
    [DataRow(0, 100, 101)]
    // target is instruction's address (0) + 1 (PC+1 per instruction), + 65536 (normalize negative address)
    [DataRow(-1, -10, 0 + 1 - 10 + 65536)]
    [DataRow(1, 100, 1)]
    // target is instruction's address (0) + 1 (PC+1 per instruction), + 65536 (normalize negative address)
    [DataRow(-1, -10, 0 + 1 - 10 + 65536)]
    public void TestBleInstruction(int testValue, int offset, int expectedPc)
    {
        var cpu = new AnnaMachine();
        cpu.Registers[1] = (Word)(uint)testValue;

        var instruction = Instruction.Ble(1, offset);
        var newPc = cpu.ExecuteImm8Type(instruction);

        Assert.AreEqual((uint)expectedPc, newPc);
    }

    [TestMethod]
    [DataRow(0u, 0xc0, 0xffc0u)]
    [DataRow(0u, -1, 0xffffu)]
    [DataRow(0u, 0x23, 0x0023u)]
    public void TestLliInstruction(uint orig, int imm8, uint expected)
    {
        var cpu = new AnnaMachine();
        cpu.Registers[1] = orig;
        var instruction = Instruction.Lli(1, imm8);

        var newPc = cpu.ExecuteImm8Type(instruction);

        Assert.AreEqual((Word)expected, cpu.Registers[1]);
    }

    [TestMethod]
    [DataRow(0u, 0xc0, 0xc000u)]
    [DataRow(0u, -1, 0xff00u)]
    [DataRow(0xffffu, 0x23, 0x23ffu)]
    public void TestLuiInstruction(uint orig, int imm8, uint expected)
    {
        var cpu = new AnnaMachine();
        cpu.Registers[1] = orig;
        var instruction = Instruction.Lui(1, imm8);

        var newPc = cpu.ExecuteImm8Type(instruction);

        Assert.AreEqual((Word)expected, cpu.Registers[1]);
    }
}