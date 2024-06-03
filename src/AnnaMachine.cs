using AnnaSim.Extensions;

public class AnnaMachine
{
    public Queue<Word> Inputs { get; internal set; } = [];
    public MemoryFile Memory { get; internal set; } = new();
    public RegisterFile Registers { get; internal set; } = new();
    public Action<Word> OutputCallback { get; set; } = (w) => Console.WriteLine($"out: {w}");

    public uint Pc { get; internal set; } = 0;

    public AnnaMachine() { }

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

    public AnnaMachine Execute(int maxCyles = 10_000)
    {
        Pc = 0;

        while (maxCyles > 0)
        {
            // Fetch and Decode
            var instruction = new Instruction(Memory[Pc]);

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

            maxCyles--;
        }

        return this;
    }

    internal void ExecuteRType(Instruction instruction)
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
        }
        else if (instruction.Opcode == Opcode.Jalr)
        {
            Registers[instruction.Rs1] = Pc + 1;
            Pc = Registers[instruction.Rd];
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
        }
        else if (instruction.Opcode == Opcode.Out)
        {
            var value = Registers[instruction.Rd];
            OutputCallback(value);
        }
    }

    internal void ExecuteImm6Type(Instruction instruction)
    {
        if (instruction.Opcode == Opcode.Addi)
        {
            SignedWord rvalue = Registers[instruction.Rs1];
            SignedWord immvalue = (SignedWord)instruction.Imm6;
            SignedWord result = rvalue + immvalue;
            Registers[instruction.Rd] = result;
        }
        else if (instruction.Opcode == Opcode.Addi)
        {
            throw new NotImplementedException();
        }
        else if (instruction.Opcode == Opcode.Addi)
        {
            throw new NotImplementedException();
        }
        else if (instruction.Opcode == Opcode.Addi)
        {
            throw new NotImplementedException();
        }
    }

    internal void ExecuteImm8Type(Instruction instruction)
    {
        throw new NotImplementedException();
    }
}