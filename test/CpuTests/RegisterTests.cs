using AnnaSim.Cpu.Memory;

namespace AnnaSim.Test.CpuTests;

[TestClass]
public class RegisterTests
{
    [TestMethod]
    public void ReadWriteR0()
    {
        var r = new RegisterFile();
        r[0] = 23;  // any old number
        Assert.IsTrue(r[0] == 0);
    }

    [TestMethod]
    public void ReadWriteOtherRegisters()
    {
        var vals = new uint[7] { 0x11, 0x21, 0x31, 0x41, 0x51, 0x61, 0x71 };
        var r = new RegisterFile();

        for (var i = 0u; i < vals.Length; i++)
        {
            r[i + 1] = vals[i];
        }

        Assert.IsTrue(r[3] == 0x31);
    }
}