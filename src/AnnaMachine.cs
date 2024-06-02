using System.Dynamic;

public class AnnaMachine
{
    internal MemoryFile memory = new();
    internal RegisterFile registers = new();

    public ushort Pc { get; internal set; } = 0;

    public AnnaMachine ReadMemFile(string path)
    {
        memory.ReadMemFile(path);
        return this;
    }

    public AnnaMachine Execute(int maxCyles = 10_000)
    {
        Pc = 0;

        while (maxCyles > 0)
        {
            // Fetch and Decode
            var instruction = new Instruction(memory[Pc]);

            // Execute (store is handled here)
            if (instruction.IsHalt)
            {
                break;
            }

            switch (instruction.Type)
            {
                case InstructionType.R:
                    ExecuteTypeR(instruction);
                    break;
                case InstructionType.Imm6:
                    ExecuteTypeImm6(instruction);
                    break;
                case InstructionType.Imm8:
                    ExecuteTypeImm8(instruction);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            maxCyles--;
        }

        return this;
    }

    internal void ExecuteTypeR(Instruction instruction)
    {
        if (instruction.Opcode == Opcode._Math)
        {
            var rs1 = instruction.Rs1;
            var rs2 = instruction.Rs2;
            var rs1val = (SignedWord)registers[rs1];
            var rs2val = (SignedWord)registers[rs2];

            SignedWord rdval = instruction.FuncCode switch
            {
                MathOp.Add => rs1val + rs2val,
                MathOp.Sub => rs1val - rs2val,
                MathOp.And => rs1val & rs2val,
                MathOp.Or => rs1val | rs2val,
                MathOp.Not => ~rs1val,
                _ => throw new InvalidOperationException()
            };

            registers[instruction.Rd] = (ushort)rdval;
        }
    }

    internal void ExecuteTypeImm6(Instruction instruction)
    {
        throw new NotImplementedException();
    }

    internal void ExecuteTypeImm8(Instruction instruction)
    {
        throw new NotImplementedException();
    }
}