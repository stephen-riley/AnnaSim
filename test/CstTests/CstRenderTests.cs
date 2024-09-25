using AnnaSim.AsmParsing;
using AnnaSim.Extensions;

namespace AnnaSim.Test.CstTests;

[TestClass]
public class CstRenderTests
{
    // This is going to look odd... the rendering will have standard column
    // widths, whereas the original source may not.  To compare them, we'll
    // simply squash all whitespace except newlines (since we do want BlankLine
    // trivia to put in actual blank lines).
    private static string Squash(string s) => s.Trim().Replace(" ", "").Replace("\t", "");

    [TestMethod]
    public void TestSimpleRender()
    {
        var source = """
        # start

            add r0 r1 r2

        # end
        """;

        var instructions = CstParser.ParseSource(source);

        var stream = new AnnaStringWriter();
        foreach (var i in instructions)
        {
            i.Render(stream);
        }

        var output = stream.ToString();

        Assert.AreEqual(Squash(source), Squash(output));
    }

    [TestMethod]
    public void TestComplexReader()
    {
        var source = File.ReadAllText("fixtures/cst_test_file2.asm");

        var instructions = CstParser.ParseSource(source);

        var stream = new AnnaStringWriter();
        foreach (var i in instructions)
        {
            i.Render(stream);
        }

        var output = stream.ToString();

        Assert.AreEqual(Squash(source), Squash(output));
    }
}