namespace AnnaSim.Instructions.Definitions;

public partial class BneInstruction : InstructionDefinition
{
    public BneInstruction() : base()
    {
        Opcode = 11;
        Mnemonic = "bne";
        OperandCount = 2;
        Type = InstructionType.Imm8;
        MathOp = MathOperation.NA;
        FormatString = "md18";
    }
}

