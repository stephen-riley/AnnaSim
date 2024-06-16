using AnnaSim.Assember;
using AnnaSim.Extensions;

namespace AnnaSim.Test.AssemblerTests;

[TestClass]
public class AssemblerTests
{
    [TestMethod]
    public void TestFullAssembly()
    {
        var lines = File.ReadAllLines("files/fibonacci.asm");
        var asm = new AnnaAssembler();

        asm.Assemble(lines);
        var i = Enumerable.Range(0, 16).Select(addr => asm.MemoryImage[(uint)addr].ToInstruction().ToString());

        Assert.AreEqual(15u, asm.Addr);
    }
}