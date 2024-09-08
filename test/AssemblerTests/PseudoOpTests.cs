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
        Assert.AreEqual(0b0111_001_111_000000, asm.MemoryImage[0]);

        // addi r7 r7 -1
        Assert.AreEqual(0b0100_111_111_111111, asm.MemoryImage[1]);
    }

    [TestMethod]
    public void TestPopPseudoOp()
    {
        var src = """
            pop    r7 r1
        """.Split('\n');

        var asm = new AnnaAssembler();
        asm.Assemble(src);

        // addi r7 r7 1
        Assert.AreEqual(0b0100_111_111_000001, asm.MemoryImage[0]);

        // sw r7 r1 0
        Assert.AreEqual(0b0110_001_111_000000, asm.MemoryImage[1]);
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

    [TestMethod]
    public void TestMovPseudoOp()
    {
        var src = """
            mov     r1 r2
        """.Split('\n');

        var asm = new AnnaAssembler();
        asm.Assemble(src);

        // add r1 r2 0
        Assert.AreEqual(0b0000_001_010_000_000, asm.MemoryImage[0]);
    }
}