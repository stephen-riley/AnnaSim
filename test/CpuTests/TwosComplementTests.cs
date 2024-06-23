using AnnaSim.Cpu;
using AnnaSim.Instructions;
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

        var idef = I.Lookup["add"];
        var instruction = idef.ToInstruction(rd: 1, rs1: 2, rs2: 3);
        idef.Execute(cpu, instruction);

        Assert.AreEqual((SignedWord)result, (SignedWord)cpu.Registers[1]);
    }

    [TestMethod]
    [DataRow(0b0100_000_000_000000u, 0)]
    [DataRow(0b0100_001_010_111011u, -5)]
    [DataRow(0b0100_001_010_000101u, 5)]
    public void TestImm6Values(uint bits, int imm6Value)
    {
        var instruction = I.Instruction((ushort)bits);
        Assert.AreEqual(imm6Value, instruction.Imm6);
    }

    [TestMethod]
    [DataRow(65535u, 1, 0u)]
    [DataRow(0u, -5, 65531u)]
    [DataRow(4294967295u, 0, 65535u)]
    public void TestNormalizePc(uint baseAddr, int offset, uint expected)
    {
        // grab a random idef
        var idef = I.Lookup["add"];
        var cpu = new AnnaMachine();
        idef.Cpu = cpu;

        var addr = idef.NormalizePc((int)baseAddr + offset);

        Assert.AreEqual(expected, addr);
    }
}