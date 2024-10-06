using AnnaSim.Instructions;
using AnnaSim.Cpu.Memory;
using AnnaSim.Extensions;
using AnnaSim.Assembler;
using AnnaSim.Exceptions;
using AnnaSim.AsmParsing;

namespace AnnaSim.Cpu;
public class AnnaMachine
{
    public Queue<Word> Inputs { get; internal set; } = [];
    public MemoryFile Memory { get; internal set; } = new();
    public PdbInfo Pdb { get; internal set; } = new();
    public CstProgram Program { get; internal set; }
    public RegisterFile Registers { get; internal set; } = new();
    public Action<Word> OutputCallback { get; set; } = (w) => Console.WriteLine($"out: {w}");
    public Action<string> OutputStringCallback { get; set; } = (w) => Console.WriteLine($"out: {w}");
    public CpuStatus Status { get; internal set; } = CpuStatus.Halted;
    public int CyclesExecuted { get; internal set; } = 0;

    public uint Pc { get; internal set; } = 0;
    public Word MemoryAtPc => Memory[Pc];
    public bool IsPcBreakpoint => Memory.Get32bits(Pc).IsBreakpoint;
    public bool Trace { get; set; }

    private ushort[] lastRegisterValues = [];

    public AnnaMachine()
    {
        Program = new([]);
    }

    public AnnaMachine(CstProgram program) : this()
    {
        Program = program;
        Memory = program.MemoryImage ?? throw new NullReferenceException($"{nameof(program)} should not be null here");
    }

    public AnnaMachine(CstProgram program, params string[] inputs)
        : this(program)
    {
        ParseMachineInputs(inputs).ForEach(Inputs.Enqueue);
    }

    public AnnaMachine(CstProgram program, params uint[] inputs) : this(program)
    {
        inputs.Select(n => (Word)(ushort)n).ForEach(Inputs.Enqueue);
    }

    internal AnnaMachine(int[] inputs) : this()
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

    public AnnaMachine Reset() => Reset([]);

    public AnnaMachine Reset(params uint[] inputs)
    {
        InstructionDefinition.SetCpu(this);

        Registers = new();

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

    int maxLabelLength = 6;
    int maxInstructionLength = 18;

    void DumpTraceInstruction(Instruction? instr)
    {
        Program.AddrMap.TryGetValue(Pc, out var ci);

        // render current instruction
        if (ci is not null)
        {
            Console.Error.Write(ci.RenderSimpleInstruction(ref maxLabelLength, ref maxInstructionLength));
        }
        else
        {
            Console.Error.Write($"[{Pc:x4}] {instr,-58}");
        }

        Console.Error.Write("  |  ");
    }

    void DumpTraceResults()
    {
        // render changed register
        var changedRegister = Enumerable.Range(0, Registers.Length).Aggregate(-1, (changed, index) => lastRegisterValues[index] != Registers[(uint)index].bits ? index : changed);
        if (changedRegister > -1)
        {
            Console.Error.Write($"r{changedRegister}: 0x{Registers[(uint)changedRegister].bits:x4}");
        }
        else
        {
            Console.Error.Write(new string(' ', 10));
        }

        if (Registers[7] != 0)
        {
            Console.Error.Write($"  |  SP:{Registers[7].bits:x4}");

            const uint stackDepth = 8;
            var stackStart = 0x8000 - Registers[7] > stackDepth ? Registers[7] - stackDepth : 0x8000;

            for (uint addr = stackStart; addr >= stackStart - stackDepth; addr--)
            {
                if (addr == Registers[7])
                {
                    break;
                }

                Console.Error.Write($" {Memory[addr]:x4}");
            }
        }

        Console.Error.WriteLine();
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
                lastRegisterValues = Registers.registers.Select(r => r.bits).ToArray();

                var instruction = ISA.Instruction((Word)mw);

                if (instruction.IsHalt)
                {
                    break;
                }

                if (Trace)
                {
                    DumpTraceInstruction(instruction);
                }

                Pc = instruction.Execute();
                CyclesExecuted++;

                if (Trace)
                {
                    DumpTraceResults();
                }

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