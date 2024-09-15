using AnnaSim.Instructions;
using AnnaSim.Cpu.Memory;
using AnnaSim.Extensions;
using AnnaSim.Assembler;
using AnnaSim.Exceptions;

namespace AnnaSim.Cpu;
public class AnnaMachine
{
    public Queue<Word> Inputs { get; internal set; } = [];
    public MemoryFile Memory { get; internal set; } = new();
    public PdbInfo Pdb { get; internal set; } = new();
    public RegisterFile Registers { get; internal set; } = new();
    public Action<Word> OutputCallback { get; set; } = (w) => Console.WriteLine($"out: {w}");
    public Action<string> OutputStringCallback { get; set; } = (w) => Console.WriteLine($"out: {w}");
    public CpuStatus Status { get; internal set; } = CpuStatus.Halted;
    public string CurrentFile { get; internal set; } = "";
    public int CyclesExecuted { get; internal set; } = 0;

    public uint Pc { get; internal set; } = 0;
    public Word MemoryAtPc => Memory[Pc];
    public bool IsPcBreakpoint => Memory.Get32bits(Pc).IsBreakpoint;

    public AnnaMachine()
    {
    }

    public AnnaMachine(string filename) : this()
    {
        CurrentFile = filename;
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
        var words = new List<Word>();

        foreach (var s in inputs)
        {
            var radix = s.Length < 2
                ? 10
                : s[0..2] switch
                {
                    "0x" or "0X" => 16,
                    "0b" or "0B" => 2,
                    _ => 10
                };

            var w = Convert.ToInt16(s.Substring(radix == 10 ? 0 : 2), radix);

            words.Add((ushort)w);
        }

        return words;
    }

    public AnnaMachine(params string[] inputs) : this() => ParseMachineInputs(inputs).ForEach(Inputs.Enqueue);

    public AnnaMachine Reset() => Reset("");

    public AnnaMachine Reset(params uint[] inputs) => Reset("", inputs);

    public AnnaMachine Reset(string filename, params uint[] inputs)
    {
        InstructionDefinition.SetCpu(this);

        Registers = new();

        CurrentFile = filename;

        if (CurrentFile.EndsWith(".mem"))
        {
            Memory = new MemoryFile().ReadMemFile(CurrentFile);
            Pdb = new();
        }
        else
        {
            var asm = new AnnaAssembler(CurrentFile);
            Memory = asm.MemoryImage;
            Pdb = asm.GetPdb();
        }

        Pc = 0;
        CyclesExecuted = 0;
        Status = CpuStatus.Halted;

        inputs.ForEach(i => Inputs.Enqueue(i));
        return this;
    }

    public AnnaMachine ReadMemFile(string path)
    {
        Memory.ReadMemFile(path);
        return this;
    }

    public void ClearInputs() => Inputs.Clear();

    public AnnaMachine AddInputs(IEnumerable<string> inputs) => AddInputs(ParseMachineInputs(inputs.ToArray()));

    public AnnaMachine AddInputs(IEnumerable<Word> inputs)
    {
        inputs.Select(n => (Word)(ushort)n).ForEach(Inputs.Enqueue);
        return this;
    }

    // NOTE: Reset() must be called first!
    public HaltReason ExecuteSingleInstruction()
    {
        var status = Execute(1);

        return status switch
        {
            HaltReason.CyclesExceeded => HaltReason.DebuggerSingleStep,
            _ => status
        };
    }

    public HaltReason Execute(int maxCycles = 10_000)
    {
        while (maxCycles > 0)
        {
            // Fetch and Decode
            var mw = Memory.Get32bits(Pc);
            if (mw.IsBreakpoint && Status is not CpuStatus.Paused)
            {
                Status = CpuStatus.Paused;
                return HaltReason.Breakpoint;
            }

            Status = CpuStatus.Running;

            try
            {
                var instruction = ISA.Instruction((Word)mw);

                if (instruction.IsHalt)
                {
                    break;
                }

                Pc = instruction.Execute();
                CyclesExecuted++;

                if (Status == CpuStatus.Halted)
                {
                    return HaltReason.Halted;
                }

                maxCycles--;
            }
            catch (InvalidOpcodeException e)
            {
                throw new InvalidOperationException($"Invalid instruction at 0x{Pc:x4}", e);
            }
        }

        if (maxCycles == 0)
        {
            Status = CpuStatus.Paused;
            return HaltReason.CyclesExceeded;
        }

        return HaltReason.Halted;
    }

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
}