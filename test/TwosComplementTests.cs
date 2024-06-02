namespace AnnaSim.Test;

[TestClass]
public class TwosComplementTests
{
    [TestMethod]
    [DataRow(1, 1, 2)]
    [DataRow(-2, -10, -12)]
    [DataRow(-1, 1, 0)]
    [DataRow(1, -1, 0)]
    [DataRow(32767, 1, -32768)]
    [DataRow(-32768, -1, 32767)]
    public void TestAdditionOfNegatives(int o1, int o2, int result)
    {
        var cpu = new AnnaMachine();
        cpu.registers[2] = (SignedWord)o1;
        cpu.registers[3] = (SignedWord)o2;

        var instruction = new Instruction(Opcode._Math, 1, 2, 3, MathOp.Add);
        cpu.ExecuteRType(instruction);
        Assert.AreEqual((SignedWord)result, (SignedWord)cpu.registers[1]);
    }
}