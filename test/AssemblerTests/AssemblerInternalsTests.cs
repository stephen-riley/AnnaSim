using AnnaSim.Assember;
using AnnaSim.Instructions;
using AnnaSim.Cpu.Memory;
using AnnaSim.Exceptions;

namespace AnnaSim.Test.AssemblerTests;

[TestClass]
public class AssemblerInternalsTests
{
    [TestMethod]
    [DataRow("r0", 0)]
    [DataRow("-10", -10)]
    [DataRow("0xffff", 65535)]
    [DataRow("0b10000001", 129)]
    [DataRow("&label", -1)]
    public void TestParsingOperands(string o, int expected)
    {
        var asm = new AnnaAssembler();

        var value = asm.ParseOperand(o);

        Assert.AreEqual(expected, value);
    }

    [TestMethod]
    [DataRow(".halt", new string[0], new uint[] { 0x3000 }, 1, 1)]
    [DataRow(".fill", new string[] { "1", "0b10", "0x03", "&label" }, new uint[] { 1, 2, 3, 0xffff }, 4, 4)]
    [DataRow(".ralias", new string[] { "r7", "rSP" }, new uint[] { 0 }, 1, 0)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1861:Avoid constant arrays as arguments", Justification = "Attributes have weird rules about arrays as parameters")]
    public void TestGoodDirectiveHandler(string instruction, string[] operands, uint[] memImage, int memLength, int expectedPtr)
    {
        var asm = new AnnaAssembler();
        var memImageWords = memImage.Select(ui => (Word)ui).ToList();

        asm.HandleDirective(OpInfo.OpcodeMap[instruction], operands);

        Assert.AreEqual(-1, asm.MemoryImage.Compare(memImageWords));
        Assert.AreEqual((uint)expectedPtr, asm.Addr);
    }

    [TestMethod]
    [DataRow("add r2 r3 r4", 0b0000_010_011_100_000u)] // 0 for math op, r2, r3, r4, 0 as func code
    [DataRow("not r2 r3", 0b0000_010_011_111_100u)]    // 0 for math op, r2, r3, -1 (unused Rs2), 8 as func code
    public void TestGoodMathInstructionHandler(string instruction, uint expected)
    {
        var asm = new AnnaAssembler();

        var pieces = instruction.Split(' ');
        asm.HandleMathOpcode(OpInfo.OpcodeMap[pieces[0]], pieces[1..]);

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

        var pieces = instruction.Split(' ');
        asm.HandleStandardOpcode(OpInfo.OpcodeMap[pieces[0]], pieces[1..]);

        Assert.AreEqual((Word)expected, asm.MemoryImage[0]);
        Assert.AreEqual(1u, asm.Addr);
    }

    [TestMethod]
    [DataRow(".halt", new uint[] { 0x3000 }, 1)]
    [DataRow(".fill 1 0b10 0x03 &label", new uint[] { 1, 2, 3, 0xffff }, 4)]
    [DataRow(".ralias r7 rSP", new uint[] { 0 }, 0)]
    public void TestGoodDirectiveAssembler(string instruction, uint[] memImage, int expectedPtr)
    {
        var asm = new AnnaAssembler();
        var memImageWords = memImage.Select(ui => (Word)ui).ToList();

        var pieces = instruction.Split(' ');
        asm.AssembleLine(pieces);

        Assert.AreEqual(-1, asm.MemoryImage.Compare(memImageWords));
        Assert.AreEqual((uint)expectedPtr, asm.Addr);
    }

    [TestMethod]
    [DataRow("add r2 r3 r4", 0b0000_010_011_100_000u)] // 0 for math op, r2, r3, r4, 0 as func code
    [DataRow("not r2 r3", 0b0000_010_011_111_100u)]    // 0 for math op, r2, r3, -1 (unused Rs2), 8 as func code
    public void TestGoodMathInstructionAssembler(string instruction, uint expected)
    {
        var asm = new AnnaAssembler();

        var pieces = instruction.Split(' ');
        asm.AssembleLine(pieces);

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

        var pieces = instruction.Split(' ');
        asm.AssembleLine(pieces);

        Assert.AreEqual((Word)expected, asm.MemoryImage[0]);
        Assert.AreEqual(1u, asm.Addr);
    }

    [TestMethod]
    [DataRow(".halt 1", "operands required")]
    [DataRow(".fill", "operands required")]
    [DataRow(".ralias r8", "operands required")]
    [DataRow(".ralias", "operands required")]
    [DataRow(".ralias r0 Bob", "cannot parse directive")]
    public void TestBadDirectives(string instruction, string messageExcerpt)
    {
        var asm = new AnnaAssembler();

        var exception = Assert.ThrowsException<InvalidOpcodeException>(() =>
        {
            asm.AssembleLine(instruction.Split(' '));
        });

        Assert.IsTrue(exception.Message.Contains(messageExcerpt));
    }

    [TestMethod]
    [DataRow("loop: add r1 r2 r3", 1, 0)]
    [DataRow("lli r2 &addr", 0, 1)]
    public void TestLabelDeclaration(string instruction, int labels, int tbdLabels)
    {
        var asm = new AnnaAssembler();

        asm.AssembleLine(instruction.Split(' '));

        Assert.AreEqual(labels, asm.labels.Count);
        Assert.AreEqual(tbdLabels, asm.resolutionToDo.Count);
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
        asm.AssembleLine("beq r1 &label".Split(' '));
        asm.labels["label"] = targetAddr;

        Assert.AreEqual("label", asm.resolutionToDo[0]);
        Assert.IsTrue(asm.labels.ContainsKey("label"));

        asm.ResolveLabels();

        uint offset = asm.MemoryImage[0] & 0x00ff;
        Assert.AreEqual(expectedOffset, offset);
    }

    [TestMethod]
    public void TestLliLuiFromLabel()
    {
        var asm = new AnnaAssembler();
        asm.labels["label"] = 0x050c;
        asm.AssembleLine("lli r1 &label".Split(' '));
        asm.AssembleLine("lui r1 &label".Split(' '));

        asm.ResolveLabels();

        Assert.AreEqual(0x0c, asm.MemoryImage[0] & 0xff);
        Assert.AreEqual(0x05, asm.MemoryImage[1] & 0xff);
    }

    [TestMethod]
    [DataRow(0x1000u)]
    [DataRow(0xc000u)]
    public void TestBranchToFarLabel(uint targetAddr)
    {
        var asm = new AnnaAssembler();
        asm.AssembleLine("beq r1 &label".Split(' '));
        asm.labels["label"] = targetAddr;

        Assert.ThrowsException<InvalidOpcodeException>(() =>
        {
            asm.ResolveLabels();
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

        asm.AssembleLine(instruction.Split(' '));
        asm.ResolveLabels();

        Assert.AreEqual(expected, (uint)asm.MemoryImage[instrAddr]);
    }
}