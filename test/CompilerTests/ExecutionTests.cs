using AnnaSim.Assembler;
using AnnaSim.Cpu.Memory;
using AnnaSim.Debugger;
using AnnaSim.TinyC;

namespace AnnaSim.Test.CompilerTests;

[TestClass]
public class ExecutionTests
{
    private List<Word> ZeroToNine = Enumerable.Range(0, 10).Select(n => (Word)(uint)n).ToList();

    private void RunFile(string fname, List<Word> expected) => Run([], File.ReadAllText(fname), expected);
    private void RunFile(List<Word> inputs, string fname, List<Word> expected) => Run(inputs, File.ReadAllText(fname), expected);

    private void Run(List<Word> inputs, string src, List<Word> expected)
    {
        Compiler.TryCompile(src, out var asm);
        if (asm is null)
        {
            throw new NullReferenceException($"ExecutionTests.RunFile() returned no assembly source");
        }

        var assembler = new AnnaAssembler();
        var program = assembler.Assemble(asm);

        var strInputs = inputs.Select(i => i.ToString()).ToArray();
        var runner = new Runner(program, strInputs);
        runner.Run();

        CollectionAssert.AreEqual(expected, runner.Outputs);
        Assert.AreEqual(0x8000u, (uint)runner.Cpu.Registers[7]);
    }

    [TestMethod]
    public void TestUnarySigns() => RunFile("fixtures/unary.c", [10, 0xfff6]);

    [TestMethod]
    public void TestWhile() => RunFile("fixtures/while.c", ZeroToNine);

    [TestMethod]
    public void TestDoWhile() => RunFile("fixtures/do-while.c", ZeroToNine);

    // [TestMethod]
    // public void TestFor() => RunFile("fixtures/for.c", ZeroToNine);

    [TestMethod]
    public void TestPostfixOperators() => RunFile("fixtures/postfix_operators.c", [2, 9]);

    [TestMethod]
    public void TestOpEqualAssignments() => RunFile("fixtures/op_equal.c", [3, 8, 15, 8]);

    [TestMethod]
    public void TestInIntrinsic() => RunFile([6], "fixtures/simple_fibonacci.c", [8]);

    [TestMethod]
    public void TestNormalFor() => RunFile([6], "fixtures/for.c", ZeroToNine);

    [TestMethod]
    public void TestNormalForAlt1() => RunFile([6], "fixtures/for_alt1.c", ZeroToNine);

    [TestMethod]
    public void TestNormalForAlt2() => RunFile([6], "fixtures/for_alt2.c", ZeroToNine);

    [TestMethod]
    public void TestDerefOperator() => RunFile([], "fixtures/deref.c", [0x58]);
}