using AnnaSim.AsmParsing;
using AnnaSim.Assembler;
using AnnaSim.Cpu.Memory;
using AnnaSim.Exceptions;
using AnnaSim.Extensions;
using AnnaSim.Instructions;

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
    public Action? PreInstructionExecutionCallback { get; set; }
    public Action? PostInstructionExecutionCallback { get; set; }
    public CpuStatus Status { get; internal set; } = CpuStatus.Halted;
    public int CyclesExecuted { get; internal set; } = 0;

    public uint Pc { get; internal set; } = 0;
    public Word MemoryAtPc => Memory[Pc];
    public bool IsPcBreakpoint => Memory.Get32bits(Pc).IsBreakpoint;
    public bool Trace { get; set; }

    private ushort[] lastTracedRegisterValues = [];

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

    int maxLabelLength = 0;
    int maxOpcodeOperandLength = 18;
    int maxInstructionLength = 10;

    void TracePreExecution(uint tracedPc, Instruction instr)
    {
        Program.AddrMap.TryGetValue(tracedPc, out var ci);

        // If we're dealing with a CstInstruction, we only want to print the
        // *first* underlying instruction, and only show the effects after the
        // *last* underlying instruction.
        //
        // If we aren't dealing with a CstInstruction, just show it.
        if (ci is not null)
        {
            if (Pc == ci.BaseAddress)
            {
                lastTracedRegisterValues = Registers.registers.Select(r => r.bits).ToArray();
                var prevMaxLength = maxInstructionLength;
                var s = ci.RenderSimpleInstruction(ref maxLabelLength, ref maxOpcodeOperandLength, ref maxInstructionLength);

                if ((prevMaxLength != maxInstructionLength) || (ci.Labels.Count > 0))
                {
                    Console.Error.WriteLine();
                }

                Console.Error.Write(s);
            }
            else
            {
                return;
            }
        }
        else
        {
            lastTracedRegisterValues = Registers.registers.Select(r => r.bits).ToArray();
            Console.Error.Write($"[{tracedPc:x4}: {Memory[tracedPc]}] {instr.ToString().ToWidth(maxInstructionLength)}");
        }

        Console.Error.Write("  |  ");
    }

    void TracePostExecution(uint tracedPc)
    {
        Program.AddrMap.TryGetValue(tracedPc, out var ci);

        // See comments in `TracePreExection` for what this is about.
        if (ci is not null && tracedPc != ci.BaseAddress + ci.AssembledWords.Count - 1)
        {
            return;
        }

        // Render changed register.  We go in reverse order because pops may change
        // both the SP (r7) and the destination register `Rd`; since the SP is usually
        // shown anyway, we want to capture the `Rd` change.
        var changedRegister = Enumerable.Range(0, Registers.Length).Reverse().Aggregate(-1, (changed, index) => lastTracedRegisterValues[index] != Registers[(uint)index].bits ? index : changed);
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
            Console.Error.Write($"  |  [SP:{Registers[7].bits:x4}]");

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

    private uint tracedPc;

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
                if (PreInstructionExecutionCallback is not null)
                {
                    PreInstructionExecutionCallback();
                }

                var instruction = ISA.Instruction((Word)mw);

                if (instruction.IsHalt)
                {
                    break;
                }

                if (Trace)
                {
                    tracedPc = Pc;
                    TracePreExecution(tracedPc, instruction);
                }

                Pc = instruction.Execute();
                CyclesExecuted++;

                if (Trace)
                {
                    TracePostExecution(tracedPc);
                }

                if (PostInstructionExecutionCallback is not null)
                {
                    PostInstructionExecutionCallback();
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