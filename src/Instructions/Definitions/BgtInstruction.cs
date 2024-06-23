namespace AnnaSim.Instructions.Definitions;

public partial class BgtInstruction : InstructionDefinition
{
    public BgtInstruction() : base()
    {
        Opcode = 12;
        Mnemonic = "bgt";
        OperandCount = 2;
        Type = InstructionType.Imm8;
        MathOp = MathOperation.NA;
        FormatString = "md8";
    }
}

