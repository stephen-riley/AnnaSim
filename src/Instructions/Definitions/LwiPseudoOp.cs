namespace AnnaSim.Instructions.Definitions;

public partial class LwiPseudoOp : InstructionDefinition
{
    public LwiPseudoOp() : base()
    {
        Opcode = 0xff;
        Mnemonic = "lwi";
        OperandCount = 2;
        Type = InstructionType.Imm8;
        MathOp = MathOperation.NA;
        FormatString = "md16";
    }
}

