
using AnnaSim.Assember;
using AnnaSim.Cpu.Instructions;
using AnnaSim.Cpu.Memory;
using AnnaSim.Extensions;

namespace AnnaSim.Cpu;
public class AnnaMachine
{
    public Queue<Word> Inputs { get; internal set; } = [];
    public MemoryFile Memory { get; internal set; } = new();
    public RegisterFile Registers { get; internal set; } = new();
    public Action<Word> OutputCallback { get; set; } = (w) => Console.WriteLine($"out: {w}");

    public uint Pc { get; internal set; } = 0;

    public AnnaMachine() { }

    public AnnaMachine(string filename, params int[] inputs) : this(inputs)
    {
        var asm = new AnnaAssembler(filename);
        Memory = asm.MemoryImage;
    }

    public AnnaMachine(params int[] inputs)
    {
        inputs.Select(n => (Word)(ushort)n).ForEach(n => Inputs.Enqueue(n));
    }

    public AnnaMachine(params string[] inputs)
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

            Inputs.Enqueue((ushort)Convert.ToInt16(s.Substring(radix == 10 ? 0 : 2), radix));
        }
    }

    public AnnaMachine ReadMemFile(string path)
    {
        Memory.ReadMemFile(path);
        return this;
    }

    public HaltReason Execute(int maxCycles = 10_000)
    {
        Pc = 0;

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

            switch (instruction.Type)
            {
                case InstructionType.R:
                    ExecuteRType(instruction);
                    break;
                case InstructionType.Imm6:
                    ExecuteImm6Type(instruction);
                    break;
                case InstructionType.Imm8:
                    ExecuteImm8Type(instruction);
                    break;
                default:
                    throw new InvalidOperationException();
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

    internal void ExecuteRType(Instruction instruction) => Pc = NormalizePc(ExecuteRTypeImpl(instruction));
    internal void ExecuteImm6Type(Instruction instruction) => Pc = NormalizePc(ExecuteImm6TypeImpl(instruction));
    internal void ExecuteImm8Type(Instruction instruction) => Pc = NormalizePc(ExecuteImm8TypeImpl(instruction));

    internal uint ExecuteRTypeImpl(Instruction instruction)
    {
        if (instruction.Opcode == Opcode._Math)
        {
            var rs1 = instruction.Rs1;
            var rs2 = instruction.Rs2;
            var rs1val = (SignedWord)Registers[rs1];
            var rs2val = (SignedWord)Registers[rs2];

            SignedWord rdval = instruction.FuncCode switch
            {
                MathOp.Add => rs1val + rs2val,
                MathOp.Sub => rs1val - rs2val,
                MathOp.And => rs1val & rs2val,
                MathOp.Or => rs1val | rs2val,
                MathOp.Not => ~rs1val,
                _ => throw new InvalidOperationException()
            };

            Registers[instruction.Rd] = (ushort)rdval;
            return Pc + 1;
        }
        else if (instruction.Opcode == Opcode.Jalr)
        {
            Registers[instruction.Rs1] = Pc + 1;
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
            return Pc + 1;
        }
        else if (instruction.Opcode == Opcode.Out)
        {
            var value = Registers[instruction.Rd];
            OutputCallback(value);
            return Pc + 1;
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    internal uint ExecuteImm6TypeImpl(Instruction instruction)
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

        return Pc + 1;
    }

    internal uint ExecuteImm8TypeImpl(Instruction instruction)
    {
        if (instruction.Opcode == Opcode.Lli)
        {
            throw new NotImplementedException();
        }

        if (instruction.Opcode == Opcode.Lui)
        {
            throw new NotImplementedException();
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
                var target = (int)Pc + 1 + instruction.Imm8;
                return (uint)((uint)((int)Pc + 1 + instruction.Imm8) % Memory.Length);
            }
            else
            {
                return Pc + 1;
            }
        }
        else
        {
            return Pc + 1;
        }
    }
}