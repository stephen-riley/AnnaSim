namespace AnnaSim.Instructions.Definitions;

public partial class SwInstruction : InstructionDefinition
{
    public SwInstruction() : base()
    {
        Opcode = 7;
        Mnemonic = "sw";
        OperandCount = 3;
        Type = InstructionType.Imm6;
        MathOp = MathOperation.NA;
        FormatString = "md16";
    }
}

