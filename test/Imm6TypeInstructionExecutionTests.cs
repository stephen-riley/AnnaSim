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
}