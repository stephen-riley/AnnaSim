using AnnaSim.Assembler;
using AnnaSim.Cpu.Memory;
using AnnaSim.Extensions;
using AnnaSim.Instructions;

namespace AnnaSim.Test.AssemblerTests;

[TestClass]
public class AssemblerTests
{
    private static readonly IEnumerable<string> src = """
                in      r4          # input n
                add     r1 r0 r0    # a = 0
                addi    r2 r0 1     # b = 1

        loop:   addi    r4 r4 -1    # decrement n
                beq     r4 &end     # branch if done
                add     r3 r1 r2    # c = a + b
                add     r1 r2 r0    # a = b
                add     r2 r3 r0    # b = c
                beq     r0 &loop

        end:    out     r3          # print result

                .halt
    """.Split('\n');

    private static readonly IEnumerable<Word> assembled = [
        0x2800, 0x0200, 0x4401, 0x493f, 0xa804, 0x0650, 0x0280, 0x04c0,
        0xa0fa, 0x3600, 0x3000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000
    ];

    [TestMethod]
    public void TestFullAssembly()
    {
        var asm = new AnnaAssembler();

        asm.Assemble(src);
        var i = Enumerable.Range(0, 11).Select(addr => ISA.Instruction(asm.MemoryImage[(uint)addr]).ToString());

        Assert.AreEqual(-1, asm.MemoryImage.Compare(assembled));
    }

    [TestMethod]
    public void TestLoneLabels()
    {
        var src = """
        start:
            in      r4
        end:
            halt
        """.Split('\n');

        var asm = new AnnaAssembler();
        asm.Assemble(src);
    }

    [TestMethod]
    public void TestOutString()
    {
        var src = """
                lwi     r3 &str
                outs    r3
        str:    .cstr   "hello"
        """.Split('\n');

        var asm = new AnnaAssembler();
        asm.Assemble(src);
    }

    [TestMethod]
    [DataRow("lw r1 r2 0")]
    [DataRow("lw r1 r2")]
    public void TestVariableOperandLw(string src)
    {
        var asm = new AnnaAssembler();
        asm.Assemble([src]);
    }

    [TestMethod]
    public void TestBlockComments()
    {
        var src = """
                .org 0x000
                /* 
                    open block comment!
                    So much fun.
                */

        stack:  .def    0x8000
        start:  lwi     r7 &stack

        /* And now we get on with our lives... */

                addi    r6 r7 +1
        """;

        var asm = new AnnaAssembler();
        var program = asm.Assemble(src);
        var instrCount = program.Instructions.ThatOccupyMemory().Count();
        Assert.AreEqual(1, instrCount);
    }
}