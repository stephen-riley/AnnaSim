namespace AnnaSim.Test;

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

        var instruction = new Instruction(Opcode.Addi, 1, 2, (short)imm6);
        cpu.ExecuteImm6Type(instruction);

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

        var instruction = new Instruction(Opcode.Shf, 1, 2, (short)imm6);
        cpu.ExecuteImm6Type(instruction);

        Assert.AreEqual((Word)(ushort)result, cpu.Registers[1]);
    }
}