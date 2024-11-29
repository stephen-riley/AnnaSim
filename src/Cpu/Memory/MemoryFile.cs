namespace AnnaSim.Cpu.Memory;

public class MemoryFile
{
    // At this point we don't care about image file versions
    private const string ImageFileHeader = "# ANNA-IMG";

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
            memory[n] = (memory[n].bits & 0xffff0000) | value;
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

    public void Initialize(uint baseAddr, string str)
    {
        foreach (var c in str)
        {
            memory[baseAddr++] = c;
        }
        memory[baseAddr] = 0;
    }

    public void SetBreakpoint(uint addr) => Set32bits(addr, (uint)Get32bits(addr) | 0x80000000);

    public void ClearBreakpoint(uint addr) => Set32bits(addr, (uint)Get32bits(addr) & 0x7fffffff);

    public void SetOrClearBreakpoint(uint addr)
    {
        if (((uint)Get32bits(addr) & 0x80000000) != 0)
        {
            ClearBreakpoint(addr);
        }
        else
        {
            SetBreakpoint(addr);
        }
    }

    public MemoryFile ReadMemFile(string path)
    {
        var addr = 0u;
        var foundHeader = false;

        foreach (var line in File.ReadAllLines(path))
        {
            if (!foundHeader)
            {
                foundHeader = line.StartsWith(ImageFileHeader) ? true : throw new InvalidOperationException($".mem files must start with {ImageFileHeader}");
                continue;
            }

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

        return this;
    }

    public void WriteMemFile(string path, uint startAddr = 0u, int length = 65536, bool writeAll = false)
    {
        if (!writeAll)
        {
            // start at the top and work down until we see a non-zero.  That will be the length.
            var a = startAddr + length - 1;
            while (a >= 0 && memory[a] == 0)
            {
                a--;
            }

            // if a is negative, then all of the checked memory is empty.
            //  just stick with the original values.
            if (a >= 0)
            {
                // a now points to the last non-zero item.
                length = (int)(a - startAddr + 1);
            }
        }

        var addr = startAddr;

        StreamWriter sw;
        var cachedConsoleOut = Console.Out;
        if (path == "-")
        {
            sw = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
            Console.SetOut(sw);
        }
        else
        {
            sw = new(path) { AutoFlush = true };
        }

        sw.WriteLine($"{ImageFileHeader}:1.0");
        sw.WriteLine($":{startAddr:X4}");

        foreach (var chunk in memory[(int)startAddr..(int)(startAddr + length)].Chunk(8))
        {
            var s = string.Join(" ", chunk.Select(w => w.ToString("X4")));
            sw.WriteLine(s);
        }

        if (path == "-")
        {
            Console.SetOut(cachedConsoleOut);
        }

        sw.Dispose();
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