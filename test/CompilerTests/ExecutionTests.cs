using AnnaSim.Assembler;
using AnnaSim.Cpu.Memory;
using AnnaSim.Debugger;
using AnnaSim.TinyC;

namespace AnnaSim.Test.CompilerTests;

[TestClass]
public class ExecutionTests
{
    private List<Word> ZeroToNine = Enumerable.Range(0, 10).Select(n => (Word)(uint)n).ToList();

    private static void RunFile(string fname, List<Word> expected) => Run([], File.ReadAllText(fname), expected);
    private static void RunFile(List<Word> inputs, string fname, List<Word> expected) => Run(inputs, File.ReadAllText(fname), expected);

    private static string[] Compile(string src)
    {
        Compiler.TryCompile(src, out var asm, optimization: 0);
        if (asm is null)
        {
            throw new NullReferenceException($"ExecutionTests.RunFile() returned no assembly source");
        }

        return asm.Split(["\r", "\n", "\r\n"], StringSplitOptions.None);
    }

    private static void Run(List<Word> inputs, string src, List<Word> expected)
    {
        var asm = Compile(src);
        Console.Error.WriteLine(string.Join("\n", asm));

        var assembler = new AnnaAssembler();
        var program = assembler.Assemble(asm);

        var strInputs = inputs.Select(i => i.ToString()).ToArray();
        var runner = new Runner(program, strInputs);
        runner.Run();

        CollectionAssert.AreEqual(expected, runner.Outputs);
        Assert.AreEqual(0x8000u, (uint)runner.Cpu.Registers[7]);
    }

    private void RunForException(List<Word> inputs, string src, string errorFragment)
    {
        try
        {
            RunFile(inputs, src, []);
        }
        catch (Exception e)
        {
            if (!e.Message.Contains(errorFragment))
            {
                Assert.Fail($"expected exception message to contain \"{errorFragment}\"");
            }
        }
    }

    [TestMethod]
    public void TestUnarySigns() => RunFile("fixtures/unary.c", [10, 0xfff6]);

    [TestMethod]
    public void TestWhile() => RunFile("fixtures/while.c", ZeroToNine);

    [TestMethod]
    public void TestDoWhile() => RunFile("fixtures/do-while.c", ZeroToNine);

    [TestMethod]
    public void TestFor() => RunFile("fixtures/for.c", ZeroToNine);

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

    [TestMethod]
    public void TestIncorrectArraySizeInLiteralDeclaration() => RunForException([], "fixtures/bad_literal_array_decl.c", "does not match element count");

    [TestMethod]
    public void TestSizeofScalar() => RunFile([], "fixtures/sizeof_scalar.c", [1]);

    [TestMethod]
    public void TestSizeofArray() => RunFile([], "fixtures/sizeof_array.c", [5]);

    [TestMethod]
    public void TestSizeofUnsizedArray() => RunFile([], "fixtures/sizeof_unsized_array.c", [5]);

    [TestMethod]
    public void TestSizeofType() => RunFile([], "fixtures/sizeof_type.c", [1]);

    [TestMethod]
    public void TestLoopOnSizeofArray() => RunFile([], "fixtures/literal_array_loop.c", [1, 2, 3, 4, 5]);

    [TestMethod]
    public void TestGlobalInitializationScalar() => RunFile([], "fixtures/global_var_init_scalar.c", [5]);

    [TestMethod]
    public void TestGlobalInitializationArray() => RunFile([], "fixtures/global_var_init_array.c", [5]);

    [TestMethod]
    public void TestArraySimpleAccess() => RunFile([], "fixtures/array_simple_access.c", [3]);

    [TestMethod]
    public void TestScalarGlobalInitialization()
    {
        var asm = Compile(File.ReadAllText("fixtures/global_var_init_scalar.c"));
        foreach (var line in asm)
        {

        }
    }
}