using AnnaSim.Assembler;
using AnnaSim.Debugger;

namespace AnnaSim.Test.AssemblerTests;

[TestClass]
public class PseudoOpTests
{
    [TestMethod]
    public void TestPushPseudoOpAssemble()
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
    public void TestPopPseudoOpAssemble()
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
    public void TestLwiPseudoOpAssemble()
    {
        var src = """
            lwi     r1 0x1234
        """.Split('\n');

        var asm = new AnnaAssembler();
        var cis = asm.Assemble(src);

        // lli r1 0x34
        Assert.AreEqual(0b1000_001_0_0011_0100, asm.MemoryImage[0]);

        // lui r1 0x12
        Assert.AreEqual(0b1001_001_0_0001_0010, asm.MemoryImage[1]);
    }

    [TestMethod]
    public void TestMovPseudoOpAssemble()
    {
        var src = """
            mov     r1 r2
        """.Split('\n');

        var asm = new AnnaAssembler();
        asm.Assemble(src);

        // add r1 r2 0
        Assert.AreEqual(0b0000_001_010_000_000, asm.MemoryImage[0]);
    }

    [TestMethod]
    public void TestHaltPseudoOpAssemble()
    {
        var src = """
            halt
        """.Split('\n');

        var asm = new AnnaAssembler();
        asm.Assemble(src);

        // out r0
        Assert.AreEqual(0b0011_000_000_000_000, asm.MemoryImage[0]);
    }

    [TestMethod]
    public void TestJmpPseudoOpAssemble()
    {
        var src = """
            jmp     r1
        """.Split('\n');

        var asm = new AnnaAssembler();
        asm.Assemble(src);

        // jalr r1 r0
        Assert.AreEqual(0b0001_001_000_000_000, asm.MemoryImage[0]);
    }

    [TestMethod]
    public void TestBrPseudoOpAssemble()
    {
        var src = """
            br     110
        """.Split('\n');

        var asm = new AnnaAssembler();
        asm.Assemble(src);

        // jalr r1 r0
        Assert.AreEqual(0b1010_000_0_01101110, asm.MemoryImage[0]);
    }

    // This sets up a scenario that a student was seeing.
    [TestMethod]
    public void TestPushPseudoOp()
    {
        var src = """
            stack:  .def    0x8000
            lwi     r7 &stack

            lli     r1 1
            lli     r2 8

            push    r7 r0       # 0x8000: 0
            push    r7 r0       # 0x7fff: 0
            push    r7 r2       # 0x7ffe: 8
            push    r7 r1       # 0x7ffd: 1
            push    r7 r0       # 0x7ffc: 0
            push    r7 r1       # 0x7ffb: 1

            out     r7          # should be 0x7ffa

            lwi     r1 0

            push    r7 r1       # 0x7ffa should be 0
            out     r7          # should be 0x7ff9

            lw      r3 r7 +1
            out     r3          # should be 0
        """;

        var asm = new AnnaAssembler();
        var program = asm.Assemble(src);
        var runner = new Runner(program, []);
        runner.Run();

        Assert.AreEqual(0, runner.Cpu.Registers[3]);
        Assert.AreEqual(0x7ffa, runner.Outputs[0]);
        Assert.AreEqual(0x7ff9, runner.Outputs[1]);
        Assert.AreEqual(0, runner.Outputs[2]);
    }
}