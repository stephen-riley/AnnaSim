namespace AnnaSim.Instructions.Definitions;

public partial class HaltPseudoOp : InstructionDefinition
{
    public HaltPseudoOp() : base()
    {
        Opcode = 0xff;
        Mnemonic = "halt";
        OperandCount = 0;
        Type = InstructionType.R;
        MathOp = MathOperation.NA;
    }
}

