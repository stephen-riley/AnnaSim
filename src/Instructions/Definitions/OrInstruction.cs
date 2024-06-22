namespace AnnaSim.Instructions.Definitions;

public partial class OrInstruction : InstructionDefinition
{
    public OrInstruction() : base()
    {
        Opcode = 0;
        Mnemonic = "or";
        OperandCount = 3;
        Type = InstructionType.R;
        MathOp = MathOperation.Or;
        FormatString = "md12";
    }
}

