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
        var results = new ConsoleDebugger("../../../fixtures/multiplication.asm", ["6", "9"], ["d r4", "c", "q"]).Run();
        CollectionAssert.AreEqual(expected, results.ToList());
    }
}