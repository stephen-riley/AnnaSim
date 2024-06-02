namespace AnnaSim.Test;

[TestClass]
public class InstructionExecutionTests
{
    [TestMethod]
    public void TestAddition()
    {
        var cpu = new AnnaMachine();
        cpu.registers[2] = 10;
        cpu.registers[3] = 20;

        // add r1, r2, r3
        var instruction = new Instruction(0b0000_001_010_011_000);
        cpu.ExecuteRType(instruction);
        Assert.AreEqual((Word)30, cpu.registers[1]);
    }
}