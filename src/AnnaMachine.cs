using System.Dynamic;

public class AnnaMachine
{
    public List<Word> Inputs { get; internal set; } = [];
    public MemoryFile Memory { get; internal set; } = new();
    public RegisterFile Registers { get; internal set; } = new();

    public uint Pc { get; internal set; } = 0;

    public AnnaMachine() { }

    public AnnaMachine(params int[] inputs)
    {
        Inputs.AddRange(inputs.Select(n => (Word)(ushort)n));
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

            Inputs.Add((ushort)Convert.ToInt16(s.Substring(radix == 10 ? 0 : 2), radix));
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
            throw new NotImplementedException();
        }
        else if (instruction.Opcode == Opcode.Out)
        {
            throw new NotImplementedException();
        }
    }

    internal void ExecuteImm6Type(Instruction instruction)
    {
        throw new NotImplementedException();
    }

    internal void ExecuteImm8Type(Instruction instruction)
    {
        throw new NotImplementedException();
    }
}