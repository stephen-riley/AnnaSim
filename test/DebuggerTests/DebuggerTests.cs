using AnnaSim.Assembler;
using AnnaSim.Cpu.Memory;
using AnnaSim.Debugger;

namespace AnnaSim.Test.DebuggerTests;

[TestClass]
public class DebuggerTests
{
    [TestMethod]
    public void TestConsoleDebuggerWithStaticInputs()
    {
        Word[] expected = [54];
        var program = new AnnaAssembler().Assemble(File.ReadAllText("../../../fixtures/multiplication.asm"));
        var results = new ConsoleDebugger(program, ["6", "9"], ["r4", "c", "q"]).Run();
        CollectionAssert.AreEqual(expected, results.ToList());
    }
}