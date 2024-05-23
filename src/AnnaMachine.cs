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
        }

        return this;
    }

    internal void ExecuteTypeR(Instruction instruction)
    {
        if (instruction.Opcode == Opcode._Math)
        {
            short rs1val = (short)instruction.Rs1;
            short rx2val = (short)instruction.Rs2;

            short rdval = instruction.FuncCode switch
            {
                MathOp.Add => (short)(rs1val + rx2val),
                MathOp.Sub => (short)(rs1val - rx2val),
                MathOp.And => (short)(instruction.Rs1 & instruction.Rs2),
                MathOp.Or => (short)(instruction.Rs1 | instruction.Rs2),
                MathOp.Not => (short)~instruction.Rs1,
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