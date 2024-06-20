using AnnaSim.Cpu.Memory;

namespace AnnaSim.Test.CpuTests;

[TestClass]
public class MemoryFileTests
{
    [TestMethod]
    public void ReadWriteMemory()
    {
        var rand = new Random();
        var addr = (uint)rand.Next(65536);
        var word = (ushort)rand.Next(65536);

        var memory = new MemoryFile();
        memory[addr] = word;

        Assert.AreEqual((Word)word, memory[addr]);
    }

    [TestMethod]
    public void ReadMemFile()
    {
        var memory = new MemoryFile();
        memory.ReadMemFile("fixtures/single_offset.mem");

        foreach (var addr in Enumerable.Range(0, 0x7fff))
        {
            Assert.AreEqual(0, memory[(uint)addr]);
        }

        foreach (var val in Enumerable.Range(0, 15))
        {
            Assert.AreEqual(val, memory[(uint)(0x8000 + val)]);
        }
    }

    [TestMethod]
    public void CompareEquivalentMemoryFiles()
    {
        var memory = new MemoryFile();
        memory.ReadMemFile("fixtures/simple.mem");

        var memory2 = new MemoryFile(Enumerable.Range(1, 6).Select(n => new Word((ushort)n)));

        Assert.AreEqual(-1, memory.Compare(memory2, 6));
    }

    [TestMethod]
    public void CompareDifferentMemoryFiles()
    {
        var memory = new MemoryFile();
        memory.ReadMemFile("fixtures/simple.mem");

        var memory2 = new MemoryFile(Enumerable.Range(1, 7).Select(n => new Word((ushort)n)));

        Assert.AreEqual(6, memory.Compare(memory2, 7));
    }

    [TestMethod]
    public void CompareMemoryFilesWithWordArray()
    {
        var memory = new MemoryFile();
        memory.ReadMemFile("fixtures/simple.mem");

        var array = new Word[] { 1, 2, 3, 4, 5, 6 };

        Assert.AreEqual(-1, memory.Compare(array));
    }

    [TestMethod]
    public void TestSetAndGetBreakpoint()
    {
        var memory = new MemoryFile();
        Assert.IsFalse(memory.Get32bits(0x1000).IsBreakpoint);
        memory.SetBreakpoint(0x1000);
        Assert.IsTrue(memory.Get32bits(0x1000).IsBreakpoint);
    }
}