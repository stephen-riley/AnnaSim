namespace AnnaSim.Cpu.Memory;

public class MemoryFile
{
    internal MachineWord[] memory;

    public int Length => memory.Length;

    public MemoryFile(int size = 65_536) => memory = new MachineWord[size];

    public MemoryFile(IEnumerable<Word> contents, int size = 65_536)
        : this(size)
    {
        int ptr = 0;
        foreach (var w in contents)
        {
            memory[ptr++] = w;
        }
    }

    // For the indexers, we will only return a Word representation.
    // When setting, we do _not_ want to change the high 16 bits
    //  (which may contain runtime information), so we just set
    //  the low 16 bits.
    public Word this[uint n]
    {
        get { return (Word)memory[(n < 0 ? n + Length : n) % Length]; }
        set
        {
            n = (uint)((n < 0 ? n + Length : n) % Length);
            memory[n] = (memory[n].bits & 0xff00) | value;
        }
    }

    public MachineWord Get32bits(uint addr) => memory[addr];

    public uint Set32bits(uint addr, uint value) => memory[addr] = value;

    public void Initialize(uint baseAddr, params Word[] words)
    {
        foreach (var w in words)
        {
            memory[baseAddr++] = w;
        }
    }

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

    public int Compare(MemoryFile mem, int compareLength = -1)
    {
        int length = compareLength != -1 ? Length : compareLength;

        for (uint i = 0; i < length; i++)
        {
            if (this[i] != mem[i])
            {
                return (int)i;
            }
        }

        return -1;
    }

    public int Compare(IEnumerable<Word> contents, uint start = 0u)
    {
        var addr = start;
        foreach (var w in contents)
        {
            if (this[addr] != w)
            {
                return (int)addr;
            }
            addr++;
        }

        return -1;
    }
}