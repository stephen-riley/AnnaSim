using AnnaSim.Assembler;

namespace AnnaSim.Test.AssemblerTests;

[TestClass]
public class DirectiveTests
{
    [TestMethod]
    public void TestFillDirective()
    {
        var src = """
            .fill   1 2 3 4 5
        """.Split('\n');

        var asm = new AnnaAssembler();
        var program = asm.Assemble(src);
        CollectionAssert.AreEqual(new int[] { 1, 2, 3, 4, 5 }, program.Instructions.First().Operands.Select(o => o.AsInt()).ToList());
    }
}