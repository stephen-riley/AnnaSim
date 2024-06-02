public class MemoryFile
{
    internal MachineWord[] memory;

    public MemoryFile(int size = 65_536) => memory = new MachineWord[size];

    // For the indexers, we will only return a Word representation.
    // When setting, we do _not_ want to change the high 16 bits
    //  (which may contain runtime information), so we just set
    //  the low 16 bits.
    public Word this[uint n]
    {
        get { return (Word)memory[n]; }
        set { memory[n] = (memory[n].bits & 0xff00) | value; }
    }

    public Word Get32bits(int addr) => memory[addr];

    public uint Set32bits(int addr, uint value) => memory[addr] = value;

    public void ReadMemFile(string path)
    {
        var addr = 0u;

        foreach (var line in File.ReadAllLines(path))
        {
            if (line.StartsWith(':'))
            {
                addr = Convert.ToUInt32(line[1..], 16);
            }
            else
            {
                foreach (var hex in line.Split(' '))
                {
                    var word = Convert.ToUInt16(hex, 16);
                    this[addr] = word;
                    addr++;
                }
            }
        }
    }

    public void WriteMemFile(string path, uint startAddr = 0u, int length = 65536)
    {
        var addr = startAddr;

        using StreamWriter sw = new(path);

        sw.WriteLine($":{startAddr:X4}");

        foreach (var chunk in memory[(int)startAddr..(int)(startAddr + length)].Chunk(8))
        {
            var s = string.Join(" ", chunk.Select(w => w.ToString("X4")));
            sw.WriteLine(s);
        }
    }
}