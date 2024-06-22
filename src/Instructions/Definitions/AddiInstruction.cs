namespace AnnaSim.Instructions.Definitions;

public partial class AddiInstruction : InstructionDefinition
{
    public AddiInstruction() : base()
    {
        Opcode = 4;
        Mnemonic = "addi";
        OperandCount = 3;
        Type = InstructionType.Imm6;
        MathOp = MathOperation.NA;
        FormatString = "md16";
    }
}

