namespace AnnaSim.Instructions.Definitions;

public partial class MulInstruction : InstructionDefinition
{
    public MulInstruction() : base()
    {
        Opcode = 0;
        Mnemonic = "mul";
        OperandCount = 3;
        Type = InstructionType.R;
        MathOp = MathOperation.Mul;
        FormatString = "md12";
    }
}

