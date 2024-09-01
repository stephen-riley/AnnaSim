namespace AnnaSim.Instructions.Definitions;

public partial class MovPseudoOp : InstructionDefinition
{
    public MovPseudoOp() : base()
    {
        Opcode = 0xff;
        Mnemonic = "mov";
        OperandCount = 2;
        Type = InstructionType.R;
        MathOp = MathOperation.NA;
        FormatString = "md16";
    }
}

