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
        var results = ConsoleDebugger.Run("../../../fixtures/multiplication.asm", ["6", "9"], ["d r4", "c", "q"]);
        CollectionAssert.AreEqual(expected, results.ToList());
    }
}