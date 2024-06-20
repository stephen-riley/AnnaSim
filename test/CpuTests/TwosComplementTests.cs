using AnnaSim.Cpu;
using AnnaSim.Cpu.Instructions;
using AnnaSim.Cpu.Memory;

namespace AnnaSim.Test.CpuTests;

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
        cpu.Registers[2] = (SignedWord)o1;
        cpu.Registers[3] = (SignedWord)o2;

        var instruction = Instruction.Add(1, 2, 3);
        cpu.ExecuteRType(instruction);
        Assert.AreEqual((SignedWord)result, (SignedWord)cpu.Registers[1]);
    }

    [TestMethod]
    [DataRow(0b0100_000_000_000000, 0)]
    [DataRow(0b0100_001_010_111011, -5)]
    [DataRow(0b0100_001_010_000101, 5)]
    public void TestImm6Values(int bits, int imm6Value)
    {
        var instruction = new Instruction((ushort)bits);
        Assert.AreEqual(imm6Value, instruction.Imm6);
    }

    [TestMethod]
    [DataRow(65535u, 1, 0u)]
    [DataRow(0u, -5, 65531u)]
    [DataRow(4294967295u, 0, 65535u)]
    public void TestNormalizePc(uint baseAddr, int offset, uint expected)
    {
        int a_weird_variable = 1;
        Console.WriteLine(a_weird_variable);
        var cpu = new AnnaMachine();
        var addr = cpu.NormalizePc((int)baseAddr + offset);
        Assert.AreEqual((uint)expected, addr);
    }
}