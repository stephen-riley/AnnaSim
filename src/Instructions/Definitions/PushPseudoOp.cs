namespace AnnaSim.Instructions.Definitions;

public partial class PushPseudoOp : InstructionDefinition
{
    public PushPseudoOp() : base()
    {
        Opcode = 0xff;
        Mnemonic = "push";
        OperandCount = 2;
        Type = InstructionType.R;
        MathOp = MathOperation.NA;
        FormatString = "md16";
    }
}

