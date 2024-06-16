using AnnaSim.Cpu.Instructions;
using AnnaSim.Exceptions;

namespace AnnaSim.Test.CpuTests;

[TestClass]
public class InstructionTests
{
    [TestMethod]
    public void TestMathRTypeConstructor()
    {
        var instruction = Instruction.Not(1, 2, 3);
        Assert.AreEqual(Opcode._Math, instruction.Opcode);
        Assert.AreEqual(1u, instruction.Rd);
        Assert.AreEqual(2u, instruction.Rs1);
        Assert.AreEqual(3u, instruction.Rs2);
        Assert.AreEqual(MathOp.Not, instruction.FuncCode);
    }

    [TestMethod]
    public void TestRTypeConstructor()
    {
        var instruction = Instruction.Jalr(1, 2);
        Assert.AreEqual(Opcode.Jalr, instruction.Opcode);
        Assert.AreEqual(1u, instruction.Rd);
        Assert.AreEqual(2u, instruction.Rs1);
        Assert.AreEqual(0u, instruction.Rs2);
    }

    [TestMethod]
    public void TestImm6Constructor()
    {
        // putting an unsigned 63 in imm6 should end up being a -1
        var instruction = Instruction.Addi(1, 2, 63);
        Assert.AreEqual(Opcode.Addi, instruction.Opcode);
        Assert.AreEqual(1u, instruction.Rd);
        Assert.AreEqual(2u, instruction.Rs1);
        Assert.AreEqual(-1, instruction.Imm6);
    }

    [TestMethod]
    public void TestImm8Constructor()
    {
        var instruction = Instruction.Lui(1, 255);
        Assert.AreEqual(Opcode.Lui, instruction.Opcode);
        Assert.AreEqual(1u, instruction.Rd);
        Assert.AreEqual(-1, instruction.Imm8);
    }

    [TestMethod]
    public void TestRTypeInstructionDecode()
    {
        var instruction = new Instruction(0b0000_001_010_011_100);
        Assert.AreEqual(1u, instruction.Rd);
        Assert.AreEqual(2u, instruction.Rs1);
        Assert.AreEqual(3u, instruction.Rs2);
        Assert.AreEqual(MathOp.Not, instruction.FuncCode);
        Assert.AreEqual(Opcode._Math, instruction.Opcode);
    }

    [TestMethod]
    public void TestImm6InstructionDecode()
    {
        var instruction = new Instruction(0b0100_001_010_011101);
        Assert.AreEqual(1u, instruction.Rd);
        Assert.AreEqual(2u, instruction.Rs1);
        Assert.AreEqual(29, instruction.Imm6);
        Assert.AreEqual(Opcode.Addi, instruction.Opcode);
    }

    [TestMethod]
    public void TestImm8InstructionDecode()
    {
        // 0b10101001 is 0xA9, 169.  In imm8, that is -87.
        var instruction = new Instruction(0b1010_001_0_10101001);
        Assert.AreEqual(1u, instruction.Rd);
        Assert.AreEqual(-87, instruction.Imm8);
        Assert.AreEqual(Opcode.Beq, instruction.Opcode);
    }

    [TestMethod]
    public void TestInvalidRTypeFieldAccess()
    {
        var instruction = Instruction.Not(1, 2, 3);
        Assert.ThrowsException<InvalidInstructionFieldAccessException>(() => { _ = instruction.Imm6; });
        Assert.ThrowsException<InvalidInstructionFieldAccessException>(() => { _ = instruction.Imm8; });
    }

    [TestMethod]
    public void TestInvalidImm6TypeFieldAccess()
    {
        var instruction = Instruction.Addi(1, 2, 67);
        Assert.ThrowsException<InvalidInstructionFieldAccessException>(() => { _ = instruction.Rs2; });
        Assert.ThrowsException<InvalidInstructionFieldAccessException>(() => { _ = instruction.Imm8; });
        Assert.ThrowsException<InvalidInstructionFieldAccessException>(() => { _ = instruction.FuncCode; });
    }

    [TestMethod]
    public void TestInvalidImm8TypeFieldAccess()
    {
        var instruction = Instruction.Lui(1, 255);
        Assert.ThrowsException<InvalidInstructionFieldAccessException>(() => { _ = instruction.FuncCode; });
        Assert.ThrowsException<InvalidInstructionFieldAccessException>(() => { _ = instruction.Rs1; });
        Assert.ThrowsException<InvalidInstructionFieldAccessException>(() => { _ = instruction.Rs2; });
        Assert.ThrowsException<InvalidInstructionFieldAccessException>(() => { _ = instruction.Imm6; });
    }

    [TestMethod]
    [DataRow("add r1 r2 r3", 0b0000_001_010_011_000u)]
    [DataRow("sub r1 r2 r3", 0b0000_001_010_011_001u)]
    [DataRow("and r1 r2 r3", 0b0000_001_010_011_010u)]
    [DataRow("or r1 r2 r3", 0b0000_001_010_011_011u)]
    [DataRow("not r1 r2", 0b0000_001_010_111_100u)]
    [DataRow("jalr r1 r2", 0b0001_001_010_111_000u)]
    [DataRow("jalr r1", 0b0001_001_000_000_000u)]
    [DataRow("in r1", 0b0010_001_000_000_000u)]
    [DataRow("out r1", 0b0011_001_000_000_000u)]
    [DataRow("addi r1 r2 -15", 0b0100_001_010_110001u)]
    [DataRow("shf r1 r2 -15", 0b0101_001_010_110001u)]
    [DataRow("lw r1 r2 -15", 0b0110_001_010_110001u)]
    [DataRow("sw r1 r2 -15", 0b0111_001_010_110001u)]
    [DataRow("lli r1 193", 0b1000_001_0_11000001u)]
    [DataRow("lui r1 193", 0b1001_001_0_11000001u)]
    [DataRow("beq r1 -63", 0b1010_001_0_11000001u)]
    [DataRow("bne r1 -63", 0b1011_001_0_11000001u)]
    [DataRow("bgt r1 -63", 0b1100_001_0_11000001u)]
    [DataRow("bge r1 -63", 0b1101_001_0_11000001u)]
    [DataRow("blt r1 -63", 0b1110_001_0_11000001u)]
    [DataRow("ble r1 -63", 0b1111_001_0_11000001u)]
    [DataRow("0x0000", 0u)]
    [DataRow(".halt", 0x3000u)]
    public void TestInstructionToString(string instr, uint bits)
    {
        Assert.AreEqual(instr, new Instruction((ushort)bits).ToString());
    }
}