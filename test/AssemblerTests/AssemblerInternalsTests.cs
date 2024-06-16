using AnnaSim.Assember;
using AnnaSim.Cpu.Instructions;
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
    [DataRow("jalr r2 r5", 0b0001_010_101_111_000u)]    // jalr, r2, r5, -1 (unused RS2), func code 0
    [DataRow("in r2", 0b0010_010_111_111_000u)]         // in, r2, -1 (unused RS1) -1 (unused RS2), func code 0
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
    [DataRow("jalr r2 r5", 0b0001_010_101_111_000u)]    // jalr, r2, r5, -1 (unused RS2), func code 0
    [DataRow("shf r5 r4 -10", 0b0101_101_100_110110u)]  // shf, r5, r4, -10 as imm6
    [DataRow("beq r3 -10", 0b1010_011_0_11110110u)]     // beq, r3, unused bit, -10 as imm8
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

        Assert.AreEqual(labels, asm.labels.Count());
        Assert.AreEqual(tbdLabels, asm.resolutionToDo.Count());
    }

    [TestMethod]
    [DataRow(0xfff0u, 0xf0u)]
    [DataRow(0x0010u, 0x10u)]
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
}