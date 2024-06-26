using AnnaSim.Cpu;
using AnnaSim.Cpu.Memory;

namespace AnnaSim.Test.CpuTests;

[TestClass]
public class AnnaMachineConstructorTests
{
    [TestMethod]
    public void TestDefaultConstructor()
    {
        var cpu = new AnnaMachine();

        Assert.AreEqual(8, cpu.Registers.Length);
        Assert.AreEqual(65536, cpu.Memory.Length);
        Assert.AreEqual(0, cpu.Inputs.Count);
    }

    [TestMethod]
    public void TestNumericInputsConstructor()
    {
        var inputs = new int[] { -1, 0, 1, 2, 3 };

        var cpu = new AnnaMachine(inputs).Reset();

        Assert.AreEqual(8, cpu.Registers.Length);
        Assert.AreEqual(65536, cpu.Memory.Length);
        Assert.AreEqual(inputs.Length, cpu.Inputs.Count);
        Assert.IsTrue(Enumerable.SequenceEqual(inputs.Select(n => (Word)(uint)n), cpu.Inputs));
    }

    [TestMethod]
    public void TestStringInputsConstructor()
    {
        var sinputs = new string[] { "-1", "0x00", "0X01", "0b010", "0B0011", "4" };
        var inputs = new int[] { -1, 0, 1, 2, 3, 4 };

        var cpu = new AnnaMachine(sinputs).Reset();

        Assert.AreEqual(8, cpu.Registers.Length);
        Assert.AreEqual(65536, cpu.Memory.Length);
        Assert.AreEqual(inputs.Length, cpu.Inputs.Count);
        Assert.IsTrue(Enumerable.SequenceEqual(inputs.Select(n => (Word)(uint)n), cpu.Inputs));
    }
}