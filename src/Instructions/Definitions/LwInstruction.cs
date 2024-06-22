namespace AnnaSim.Instructions.Definitions;

public partial class LwInstruction : InstructionDefinition
{
    public LwInstruction() : base()
    {
        Opcode = 6;
        Mnemonic = "lw";
        OperandCount = 3;
        Type = InstructionType.Imm6;
        MathOp = MathOperation.NA;
        FormatString = "md16";
    }
}

