using AnnaSim.Exceptions;

namespace AnnaSim.Test;

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
        var instruction = Instruction.Addi(1, 2, 63);
        Assert.AreEqual(Opcode.Addi, instruction.Opcode);
        Assert.AreEqual(1u, instruction.Rd);
        Assert.AreEqual(2u, instruction.Rs1);
        Assert.AreEqual(63, instruction.Imm6);
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
        var instruction = new Instruction(0b1010_001_0_10101001);
        Assert.AreEqual(1u, instruction.Rd);
        Assert.AreEqual(169, instruction.Imm8);
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
}