using AnnaSim.Cpu.Memory;
using AnnaSim.Exceptions;
using AnnaSim.Assembler;
using AnnaSim.Instructions;
using AnnaSim.AsmParsing;

namespace AnnaSim.Test.AssemblerTests;

[TestClass]
public class AssemblerInternalsTests
{
    [TestMethod]
    [DataRow("-10", -10)]
    [DataRow("0xffff", 65535)]
    [DataRow("0b10000001", 129)]
    public void TestParsingOperands(string o, int expected)
    {
        var asm = new AnnaAssembler();

        var value = asm.ParseOperand(o, OperandType.SignedInt);

        Assert.AreEqual(expected, (int)value);
    }

    [TestMethod]
    [DataRow("add r2 r3 r4", 0b0000_010_011_100_000u)] // 0 for math op, r2, r3, r4, 0 as func code
    [DataRow("not r2 r3", 0b0000_010_011_000_100u)]    // 0 for math op, r2, r3, -1 (unused Rs2), 8 as func code
    public void TestGoodMathInstructionHandler(string instruction, uint expected)
    {
        var asm = new AnnaAssembler();

        asm.Assemble([instruction]);

        Assert.AreEqual((Word)expected, asm.MemoryImage[0]);
        Assert.AreEqual(1u, asm.Addr);
    }

    [TestMethod]
    [DataRow("jalr r2 r5", 0b0001_010_101_000_000u)]    // jalr, r2, r5, 0 (unused RS2), func code 0
    [DataRow("in r2", 0b0010_010_000_000_000u)]         // in, r2, 0 (unused RS1), 0 (unused RS2), func code 0
    [DataRow("shf r5 r4 -10", 0b0101_101_100_110110u)]  // shf, r5, r4, -10 as imm6
    [DataRow("beq r3 -10", 0b1010_011_0_11110110u)]     // beq, r3, unused bit, -10 as imm8
    public void TestGoodStandardInstructionHandler(string instruction, uint expected)
    {
        var asm = new AnnaAssembler();

        asm.Assemble([instruction]);

        Assert.AreEqual((Word)expected, asm.MemoryImage[0]);
        Assert.AreEqual(1u, asm.Addr);
    }

    [TestMethod]
    [DataRow(".halt", new uint[] { 0x3000 }, 1)]
    [DataRow("label: .fill 1 0b10 0x03 &label", new uint[] { 1, 2, 3, 0 }, 4)]
    // TODO: register aliases aren't implemented in the CstInstruction model yet
    // [DataRow(".ralias r7 rSP", new uint[] { 0 }, 0)]
    [DataRow("test: .def 0x8000", new uint[] { 0 }, 0)]
    [DataRow(".org 0x8000", new uint[] { 0 }, 0x8000)]
    public void TestGoodDirectiveAssembler(string instruction, uint[] memImage, int expectedPtr)
    {
        var asm = new AnnaAssembler();
        var memImageWords = memImage.Select(ui => (Word)ui).ToList();

        asm.Assemble([instruction]);

        Assert.AreEqual(-1, asm.MemoryImage.Compare(memImageWords));
        Assert.AreEqual((uint)expectedPtr, asm.Addr);
    }

    [TestMethod]
    [DataRow("add r2 r3 r4", 0b0000_010_011_100_000u)] // 0 for math op, r2, r3, r4, 0 as func code
    [DataRow("not r2 r3", 0b0000_010_011_000_100u)]    // 0 for math op, r2, r3, -1 (unused Rs2), 8 as func code
    public void TestGoodMathInstructionAssembler(string instruction, uint expected)
    {
        var asm = new AnnaAssembler();

        asm.Assemble([instruction]);

        Assert.AreEqual((Word)expected, asm.MemoryImage[0]);
        Assert.AreEqual(1u, asm.Addr);
    }

    [TestMethod]
    [DataRow("jalr r2 r5", 0b0001_010_101_000_000u)]    // jalr, r2, r5, 0 (unused RS2), func code 0
    [DataRow("shf r5 r4 -10", 0b0101_101_100_110110u)]  // shf, r5, r4, -10 as imm6
    [DataRow("beq r3 -10", 0b1010_011_0_11110110u)]     // beq, r3, unused bit, -10 as imm8
    [DataRow("lli r1 0x8000", 0b1000_001_0_00000000u)]  // lli, r1, unused bit, 0x00 as imm8
    [DataRow("lui r1 0x8000", 0b1001_001_0_10000000u)]  // lui, r1, unused bit, 0x80 as imm8
    public void TestGoodStandardInstructionAssembler(string instruction, uint expected)
    {
        var asm = new AnnaAssembler();

        asm.Assemble([instruction]);

        Assert.AreEqual((Word)expected, asm.MemoryImage[0]);
        Assert.AreEqual(1u, asm.Addr);
    }

    [TestMethod]
    [DataRow(".halt 1", "operands required")]
    [DataRow(".fill", "operands required")]
    // TODO: support register aliases in CstInstructions
    // [DataRow(".ralias r8", "operands required")]
    // [DataRow(".ralias", "operands required")]
    // [DataRow(".ralias r0 Bob", "not find any recognizable digits")]
    [DataRow(".def 0x8000", ".def must have a label")]
    [DataRow("test: .org 0x8000", ".org cannot have a label")]
    public void TestBadDirectives(string instruction, string messageExcerpt)
    {
        var asm = new AnnaAssembler();

        var exception = Assert.ThrowsException<AssemblerParseException>(() =>
        {
            asm.Assemble([instruction]);
        });

        Assert.IsTrue(exception?.InnerException?.Message.Contains(messageExcerpt));
    }

    [TestMethod]
    public void TestLabelDeclaration()
    {
        var asm = new AnnaAssembler();

        var source = """
            loop:   add r1 r2 r3
                    lli r2 &loop
        """;
        asm.Assemble(source.Split('\n'));

        Assert.AreEqual(1, asm.labels.Count);
        Assert.AreEqual(1, asm.resolutionToDo.Count);
    }

    [TestMethod]
    // Goal is to get last 8 bits of the (negative) offset:
    //  1. Calculate offset: 0 (instr addr) + 1 (PC+1 per instruction definition) - 0xfff0 (target addr)
    //  2. Bitwise negate it and add 1 for 2s complement
    //  3. Mask that with uint 0xff for last 8 bits as uint
    // This should come out to 0xef (239)
    [DataRow(0xfff0u, (~(0 + 1 - 0xfff0) + 1) & (uint)0xff)]
    [DataRow(0x0010u, 0x0fu)]
    public void TestBranchToLabel(uint targetAddr, uint expectedOffset)
    {
        var asm = new AnnaAssembler();
        asm.labels["label"] = targetAddr;
        var p = asm.Assemble(["beq r1 &label"]);

        Assert.AreEqual("&label", asm.resolutionToDo[(0, 0)]);
        Assert.IsTrue(asm.labels.ContainsKey("label"));

        asm.ResolveLabels(p.Instructions);

        uint offset = asm.MemoryImage[0] & 0x00ff;
        Assert.AreEqual(expectedOffset, offset);
    }

    [TestMethod]
    public void TestLliLuiFromLabel()
    {
        var asm = new AnnaAssembler();
        asm.labels["label"] = 0x050c;
        var p = asm.Assemble([
            "lli r1 &label",
            "lui r1 &label"
        ]);

        asm.ResolveLabels(p.Instructions);

        Assert.AreEqual(0x0c, asm.MemoryImage[0] & 0xff);
        Assert.AreEqual(0x05, asm.MemoryImage[1] & 0xff);
    }

    [TestMethod]
    public void TestLliLuiFromConstant()
    {
        var asm = new AnnaAssembler();
        asm.labels["label"] = 0x050c;
        var p = asm.Assemble([
            "lli r1 1",
            "lui r1 1"
        ]);

        asm.ResolveLabels(p.Instructions);


        Assert.AreEqual(1, asm.MemoryImage[0] & 0xff);
        Assert.AreEqual(0, asm.MemoryImage[1] & 0xff);
    }

    [TestMethod]
    [DataRow(0x1000u)]
    [DataRow(0xc000u)]
    public void TestBranchToFarLabel(uint targetAddr)
    {
        var asm = new AnnaAssembler();
        asm.labels["label"] = targetAddr;

        Assert.ThrowsException<InvalidOpcodeException>(() =>
        {
            asm.Assemble(["beq r1 &label"]);
        });
    }

    [TestMethod]
    [DataRow(0x4u, "beq r4 &target", 0x0009u, 0xa804u)]
    [DataRow(0x8u, "beq r0 &target", 0x0003u, 0xa0fau)]
    [DataRow(0x0u, "beq r0 &target", 0xfff0u, 0xa0efu)]
    public void TestBranchOffsetCalculation(uint instrAddr, string instruction, uint labelAddr, uint expected)
    {
        var asm = new AnnaAssembler();
        asm.labels["target"] = labelAddr;
        asm.Addr = instrAddr;

        var p = asm.Assemble([instruction]);
        asm.ResolveLabels(p.Instructions);

        Assert.AreEqual(expected, (uint)asm.MemoryImage[instrAddr]);
    }
}