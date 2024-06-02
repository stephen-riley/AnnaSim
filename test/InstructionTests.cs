using AnnaSim.Exceptions;

namespace AnnaSim.Test;

[TestClass]
public class InstructionTests
{
    [TestMethod]
    public void TestMathRTypeConstructor()
    {
        var instruction = new Instruction(Opcode._Math, 1, 2, 3, MathOp.Not);
        Assert.AreEqual(Opcode._Math, instruction.Opcode);
        Assert.AreEqual(1u, instruction.Rd);
        Assert.AreEqual(2u, instruction.Rs1);
        Assert.AreEqual(3u, instruction.Rs2);
        Assert.AreEqual(MathOp.Not, instruction.FuncCode);
    }

    [TestMethod]
    public void TestRTypeConstructor()
    {
        var instruction = new Instruction(Opcode.Jalr, 1, 2, 3);
        Assert.AreEqual(Opcode.Jalr, instruction.Opcode);
        Assert.AreEqual(1u, instruction.Rd);
        Assert.AreEqual(2u, instruction.Rs1);
        Assert.AreEqual(3u, instruction.Rs2);
    }

    [TestMethod]
    public void TestImm6Constructor()
    {
        var instruction = new Instruction(Opcode.Addi, 1, 2, 63);
        Assert.AreEqual(Opcode.Addi, instruction.Opcode);
        Assert.AreEqual(1u, instruction.Rd);
        Assert.AreEqual(2u, instruction.Rs1);
        Assert.AreEqual(63u, instruction.Imm6);
    }

    [TestMethod]
    public void TestImm8Constructor()
    {
        var instruction = new Instruction(Opcode.Lui, 1, 255);
        Assert.AreEqual(Opcode.Lui, instruction.Opcode);
        Assert.AreEqual(1u, instruction.Rd);
        Assert.AreEqual(255u, instruction.Imm8);
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
        Assert.AreEqual(29u, instruction.Imm6);
        Assert.AreEqual(Opcode.Addi, instruction.Opcode);
    }

    [TestMethod]
    public void TestImm8InstructionDecode()
    {
        var instruction = new Instruction(0b1010_001_0_10101001);
        Assert.AreEqual(1u, instruction.Rd);
        Assert.AreEqual(169u, instruction.Imm8);
        Assert.AreEqual(Opcode.Beq, instruction.Opcode);
    }

    [TestMethod]
    public void TestInvalidRTypeFieldAccess()
    {
        var instruction = new Instruction(Opcode._Math, 1, 2, 3, MathOp.Not);
        Assert.ThrowsException<InvalidInstructionFieldAccessException>(() => { _ = instruction.Imm6; });
        Assert.ThrowsException<InvalidInstructionFieldAccessException>(() => { _ = instruction.Imm8; });
    }

    [TestMethod]
    public void TestInvalidImm6TypeFieldAccess()
    {
        var instruction = new Instruction(Opcode.Addi, 1, 2, 67);
        Assert.ThrowsException<InvalidInstructionFieldAccessException>(() => { _ = instruction.Rs2; });
        Assert.ThrowsException<InvalidInstructionFieldAccessException>(() => { _ = instruction.Imm8; });
        Assert.ThrowsException<InvalidInstructionFieldAccessException>(() => { _ = instruction.FuncCode; });
    }

    [TestMethod]
    public void TestInvalidImm8TypeFieldAccess()
    {
        var instruction = new Instruction(Opcode.Lli, 1, 255);
        Assert.ThrowsException<InvalidInstructionFieldAccessException>(() => { _ = instruction.FuncCode; });
        Assert.ThrowsException<InvalidInstructionFieldAccessException>(() => { _ = instruction.Rs1; });
        Assert.ThrowsException<InvalidInstructionFieldAccessException>(() => { _ = instruction.Rs2; });
        Assert.ThrowsException<InvalidInstructionFieldAccessException>(() => { _ = instruction.Imm6; });
    }
}