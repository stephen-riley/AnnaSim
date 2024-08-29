using AnnaSim.Assembler;
using AnnaSim.Cpu.Memory;
using AnnaSim.Instructions;

namespace AnnaSim.Test.AssemblerTests;

[TestClass]
public class PseudoOpTests
{
    [TestMethod]
    public void TestPushPseudoOp()
    {
        var src = """
            push    r7 r1
        """.Split('\n');

        var asm = new AnnaAssembler();
        asm.Assemble(src);

        // sw r7 r1 0
        Assert.AreEqual(0b0111_111_001_000000, asm.MemoryImage[0]);

        // addi r7 r7 -1
        Assert.AreEqual(0b0100_111_111_111111, asm.MemoryImage[1]);
    }

    [TestMethod]
    public void TestLwiPseudoOp()
    {
        var src = """
            lwi     r1 0x1234
        """.Split('\n');

        var asm = new AnnaAssembler();
        asm.Assemble(src);

        // lli r1 0x34
        Assert.AreEqual(0b1000_001_0_0011_0100, asm.MemoryImage[0]);

        // lui r1 0x12
        Assert.AreEqual(0b1001_001_0_0001_0010, asm.MemoryImage[1]);
    }
}