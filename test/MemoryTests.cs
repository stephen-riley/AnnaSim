namespace AnnaSim.Test;

[TestClass]
public class MemoryTests
{
    [TestMethod]
    public void ReadWriteMemory()
    {
        var rand = new Random();
        var addr = (uint)rand.Next(65536);
        var word = (ushort)rand.Next(65536);

        var memory = new MemoryFile();
        memory[addr] = word;

        Assert.AreEqual(word, memory[addr]);
    }

    [TestMethod]
    public void ReadMemFile()
    {
        var memory = new MemoryFile();
        memory.ReadMemFile("files/single_offset.mem");

        foreach (var addr in Enumerable.Range(0, 0x7fff))
        {
            Assert.AreEqual(0, memory[(uint)addr]);
        }

        foreach (var val in Enumerable.Range(0, 15))
        {
            Assert.AreEqual(val, memory[(uint)(0x8000 + val)]);
        }
    }
}