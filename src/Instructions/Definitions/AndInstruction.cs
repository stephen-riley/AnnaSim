namespace AnnaSim.Instructions.Definitions;

public partial class AndInstruction : InstructionDefinition
{
    public AndInstruction() : base()
    {
        Opcode = 0;
        Mnemonic = "and";
        OperandCount = 3;
        Type = InstructionType.R;
        MathOp = MathOperation.And;
        FormatString = "md12";
    }
}

