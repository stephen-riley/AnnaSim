using AnnaSim.Assember;
using AnnaSim.Instructions;
using AnnaSim.Cpu.Memory;
using AnnaSim.Extensions;

namespace AnnaSim.Cpu;
public class AnnaMachine
{
    // TODO: move OrigInputs to the debugger
    private Queue<Word> OrigInputs { get; set; } = [];
    public Queue<Word> Inputs { get; internal set; } = [];
    public MemoryFile Memory { get; internal set; } = new();
    public RegisterFile Registers { get; internal set; } = new();
    public Action<Word> OutputCallback { get; set; } = (w) => Console.WriteLine($"out: {w}");
    public CpuStatus Status { get; internal set; } = CpuStatus.Initialized;
    public string CurrentFile { get; internal set; } = "";

    public uint Pc { get; internal set; } = 0;

    internal static int ParseInputString(string o)
    {
        if (o.StartsWith("0b"))
        {
            return Convert.ToInt32(o[2..], 2);
        }
        else if (o.StartsWith("0x"))
        {
            return Convert.ToInt32(o[2..], 16);
        }
        else
        {
            return Convert.ToInt32(o);
        }
    }

    public AnnaMachine() { }

    public AnnaMachine(string filename, params string[] inputs)
        : this(filename, inputs.Select(i => ParseInputString(i)).ToArray())
    {
    }

    public AnnaMachine(string filename, params int[] inputs) : this(inputs)
    {
        CurrentFile = filename;
        var asm = new AnnaAssembler(filename);
        Memory = asm.MemoryImage;
    }

    public AnnaMachine(params int[] inputs) : this()
    {
        inputs.Select(n => (Word)(ushort)n).ForEach(OrigInputs.Enqueue);
    }

    public AnnaMachine(params string[] inputs) : this()
    {
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

            OrigInputs.Enqueue((ushort)Convert.ToInt16(s.Substring(radix == 10 ? 0 : 2), radix));
        }
    }

    public AnnaMachine Reset()
    {
        Inputs = new Queue<Word>(OrigInputs);
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

            var instruction = new Instruction((Word)mw);

            // Execute (store is handled here)
            if (instruction.IsHalt)
            {
                break;
            }

            Pc = instruction.Type switch
            {
                InstructionType.R => ExecuteRType(instruction),
                InstructionType.Imm6 => ExecuteImm6Type(instruction),
                InstructionType.Imm8 => ExecuteImm8Type(instruction),
                _ => throw new InvalidOperationException(),
            };

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

    internal uint NormalizePc(int addr) => (uint)((addr < 0 ? addr + Memory.Length : addr) % Memory.Length);
    internal uint NormalizePc(uint addr) => (uint)(addr % Memory.Length);

    internal uint ExecuteRType(Instruction instruction)
    {
        if (instruction.Opcode == Opcode._Math)
        {
            var rs1 = instruction.Rs1;
            var rs2 = instruction.Rs2;
            var rs1val = (SignedWord)Registers[rs1];
            var rs2val = (SignedWord)Registers[rs2];

            SignedWord rdval = instruction.FuncCode switch
            {
                MathOperation.Add => rs1val + rs2val,
                MathOperation.Sub => rs1val - rs2val,
                MathOperation.And => rs1val & rs2val,
                MathOperation.Or => rs1val | rs2val,
                MathOperation.Not => ~rs1val,
                _ => throw new InvalidOperationException()
            };

            Registers[instruction.Rd] = (ushort)rdval;
            return NormalizePc(Pc + 1);
        }
        else if (instruction.Opcode == Opcode.Jalr)
        {
            Registers[instruction.Rs1] = NormalizePc(Pc + 1);
            return Registers[instruction.Rd];
        }
        else if (instruction.Opcode == Opcode.In)
        {
            if (Inputs.TryDequeue(out var result))
            {
                Registers[instruction.Rd] = result;
            }
            else
            {
                throw new NoInputRemainingException();
            }
            return NormalizePc(Pc + 1);
        }
        else if (instruction.Opcode == Opcode.Out)
        {
            if (instruction.Rd == 0)
            {
                Status = CpuStatus.Halted;
                return Pc;
            }

            var value = Registers[instruction.Rd];
            OutputCallback(value);
            return NormalizePc(Pc + 1);
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    internal uint ExecuteImm6Type(Instruction instruction)
    {
        if (instruction.Opcode == Opcode.Addi)
        {
            SignedWord rvalue = Registers[instruction.Rs1];
            SignedWord immvalue = (SignedWord)instruction.Imm6;
            SignedWord result = rvalue + immvalue;
            Registers[instruction.Rd] = result;
        }
        else if (instruction.Opcode == Opcode.Shf)
        {
            SignedWord rvalue = Registers[instruction.Rs1];
            SignedWord immvalue = (SignedWord)instruction.Imm6;
            SignedWord result = immvalue > 0 ? rvalue << immvalue : rvalue >> (-immvalue);
            Registers[instruction.Rd] = result;
        }
        else if (instruction.Opcode == Opcode.Lw)
        {
            int addr = Registers[instruction.Rs1];
            SignedWord immvalue = (SignedWord)instruction.Imm6;
            addr += immvalue;
            Registers[instruction.Rd] = Memory[(uint)addr];
        }
        else if (instruction.Opcode == Opcode.Sw)
        {
            int addr = Registers[instruction.Rs1];
            SignedWord immvalue = (SignedWord)instruction.Imm6;
            addr += immvalue;
            Memory[(uint)addr] = Registers[instruction.Rd];
        }

        return NormalizePc(Pc + 1);
    }

    internal uint ExecuteImm8Type(Instruction instruction)
    {
        if (instruction.Opcode == Opcode.Lli)
        {
            Registers[instruction.Rd] = (uint)instruction.Imm8.SignExtend(8);
        }

        if (instruction.Opcode == Opcode.Lui)
        {
            var rdvalue = Registers[instruction.Rd];
            rdvalue = (rdvalue & 0x00ff) | ((uint)instruction.Imm8 << 8);
            Registers[instruction.Rd] = rdvalue;
        }

        if (instruction.Opcode.IsBranch())
        {
            var condition = instruction.Opcode switch
            {
                Opcode.Beq => Registers[instruction.Rd] == 0,
                Opcode.Bne => Registers[instruction.Rd] != 0,
                Opcode.Bgt => (SignedWord)Registers[instruction.Rd] > 0,
                Opcode.Bge => (SignedWord)Registers[instruction.Rd] >= 0,
                Opcode.Blt => (SignedWord)Registers[instruction.Rd] < 0,
                Opcode.Ble => (SignedWord)Registers[instruction.Rd] <= 0,
                _ => false
            };

            if (condition)
            {
                return NormalizePc((int)Pc + 1 + instruction.Imm8);
            }
            else
            {
                return NormalizePc(Pc + 1);
            }
        }
        else
        {
            return NormalizePc(Pc + 1);
        }
    }
}