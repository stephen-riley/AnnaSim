namespace AnnaSim.Instructions.Definitions;

public partial class NotInstruction : InstructionDefinition
{
    public NotInstruction() : base()
    {
        Opcode = 0;
        Mnemonic = "not";
        OperandCount = 2;
        Type = InstructionType.R;
        MathOp = MathOperation.Not;
        FormatString = "md1";
    }
}

