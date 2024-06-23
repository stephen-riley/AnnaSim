using AnnaSim.Instructions;
using AnnaSim.Cpu.Memory;
using AnnaSim.Extensions;
using AnnaSim.Assembler;

namespace AnnaSim.Cpu;
public class AnnaMachine
{
    public Queue<Word> Inputs { get; internal set; } = [];
    public MemoryFile Memory { get; internal set; } = new();
    public RegisterFile Registers { get; internal set; } = new();
    public Action<Word> OutputCallback { get; set; } = (w) => Console.WriteLine($"out: {w}");
    public CpuStatus Status { get; internal set; } = CpuStatus.Initialized;
    public string CurrentFile { get; internal set; } = "";

    public uint Pc { get; internal set; } = 0;

    public static uint ParseInputString(string o)
    {
        if (o.StartsWith("0b"))
        {
            return Convert.ToUInt32(o[2..], 2);
        }
        else if (o.StartsWith("0x"))
        {
            return Convert.ToUInt32(o[2..], 16);
        }
        else
        {
            return Convert.ToUInt32(o);
        }
    }

    public AnnaMachine() { }

    public AnnaMachine(string filename) : this()
    {
        CurrentFile = filename;
        var asm = new AnnaAssembler(filename);
        Memory = asm.MemoryImage;
    }

    public AnnaMachine(string filename, params string[] inputs)
        : this(filename: filename)
    {
        ParseMachineInputs(inputs).ForEach(Inputs.Enqueue);
    }

    public AnnaMachine(string filename, params uint[] inputs) : this(filename: filename)
    {
        inputs.Select(n => (Word)(ushort)n).ForEach(Inputs.Enqueue);
    }

    internal AnnaMachine(int[] inputs)
    {
        inputs.Select(n => (Word)(ushort)n).ForEach(Inputs.Enqueue);
    }

    internal static IEnumerable<Word> ParseMachineInputs(params string[] inputs)
    {
        return (IEnumerable<Word>)inputs.Select(s =>
        {
            var radix = s.Length < 2
                ? 10
                : s[0..2] switch
                {
                    "0x" or "0X" => 16,
                    "0b" or "0B" => 2,
                    _ => 10
                };

            return (ushort)Convert.ToInt16(s.Substring(radix == 10 ? 0 : 2), radix);
        });
    }

    public AnnaMachine(params string[] inputs) : this() => ParseMachineInputs(inputs).ForEach(Inputs.Enqueue);

    public AnnaMachine Reset()
    {
        InstructionDefinition.SetCpu(this);

        Memory = new();
        Registers = new();

        if (CurrentFile != "")
        {
            var asm = new AnnaAssembler(CurrentFile);
            Memory = asm.MemoryImage;
        }

        Pc = 0;
        Status = CpuStatus.Initialized;
        return this;
    }

    public AnnaMachine Reset(params uint[] inputs)
    {
        Reset();
        inputs.ForEach(i => Inputs.Enqueue(i));
        return this;
    }

    public AnnaMachine ReadMemFile(string path)
    {
        Memory.ReadMemFile(path);
        return this;
    }

    // NOTE: Reset() must be called first!
    public HaltReason ExecuteSingleInstruction()
    {
        var status = Execute(1);
        return status switch
        {
            HaltReason.CyclesExceeded => HaltReason.DebuggerStep,
            _ => status
        };
    }

    public HaltReason Execute(int maxCycles = 10_000)
    {
        if (Status == CpuStatus.Halted)
        {
            return HaltReason.Halt;
        }
        else if (Status == CpuStatus.Initialized)
        {
            Reset();
            Status = CpuStatus.Running;
        }

        while (maxCycles > 0)
        {
            // Fetch and Decode
            var mw = Memory.Get32bits(Pc);
            if (mw.IsBreakpoint)
            {
                return HaltReason.Breakpoint;
            }

            var instruction = I.Instruction((Word)mw);

            // Execute (store is handled here)
            if (instruction.IsHalt)
            {
                break;
            }

            Pc = instruction.Execute();

            if (Status == CpuStatus.Halted)
            {
                return HaltReason.Halt;
            }

            maxCycles--;
        }

        if (maxCycles == 0)
        {
            return HaltReason.CyclesExceeded;
        }

        return HaltReason.Halt;
    }
}