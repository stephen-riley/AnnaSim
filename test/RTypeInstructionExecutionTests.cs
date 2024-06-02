namespace AnnaSim.Test;

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
        var instruction = new Instruction(Opcode._Math, 1, 2, 3, MathOp.Add);
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
        var instruction = new Instruction(Opcode._Math, 1, 2, 3, MathOp.Sub);
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
        var instruction = new Instruction(Opcode._Math, 1, 2, 3, MathOp.And);
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
        var instruction = new Instruction(Opcode._Math, 1, 2, 3, MathOp.Or);
        cpu.ExecuteRType(instruction);
        Assert.AreEqual((Word)0xffff, cpu.Registers[1]);
    }

    [TestMethod]
    public void TestNegation()
    {
        var cpu = new AnnaMachine();
        cpu.Registers[2] = 0xaaaa;

        // add r1, r2, r3
        var instruction = new Instruction(Opcode._Math, 1, 2, 0, MathOp.Not);
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

        var instruction = new Instruction(Opcode.Jalr, rd, rs1, 0);
        cpu.ExecuteRType(instruction);
        Assert.AreEqual(20u, cpu.Pc);
        Assert.AreEqual((Word)11, cpu.Registers[rs1]);
    }

    [TestMethod]
    public void TestJumpAndLinkRegisterAsJumpl()
    {
        // Assume PC @10, jumping @20
        var cpu = new AnnaMachine { Pc = 10 };
        ushort rd = 1;
        cpu.Registers[rd] = 20;

        var instruction = new Instruction(Opcode.Jalr, rd, 0, 0);
        cpu.ExecuteRType(instruction);
        Assert.AreEqual(20u, cpu.Pc);
    }
}