namespace AnnaSim.Instructions.Definitions;

public partial class PopPseudoOp : InstructionDefinition
{
    public PopPseudoOp() : base()
    {
        Opcode = 0xff;
        Mnemonic = "pop";
        OperandCount = 2;
        Type = InstructionType.R;
        MathOp = MathOperation.NA;
        FormatString = "md16";
    }
}

