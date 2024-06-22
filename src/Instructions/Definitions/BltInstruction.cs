namespace AnnaSim.Instructions.Definitions;

public partial class BltInstruction : InstructionDefinition
{
    public BltInstruction() : base()
    {
        Opcode = 14;
        Mnemonic = "blt";
        OperandCount = 2;
        Type = InstructionType.Imm8;
        MathOp = MathOperation.NA;
        FormatString = "md18";
    }
}

