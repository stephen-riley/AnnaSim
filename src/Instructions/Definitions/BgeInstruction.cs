namespace AnnaSim.Instructions.Definitions;

public partial class BgeInstruction : InstructionDefinition
{
    public BgeInstruction() : base()
    {
        Opcode = 13;
        Mnemonic = "bge";
        OperandCount = 2;
        Type = InstructionType.Imm8;
        MathOp = MathOperation.NA;
        FormatString = "md8";
    }
}

