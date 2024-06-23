namespace AnnaSim.Instructions.Definitions;

public partial class LliInstruction : InstructionDefinition
{
    public LliInstruction() : base()
    {
        Opcode = 8;
        Mnemonic = "lli";
        OperandCount = 2;
        Type = InstructionType.Imm8;
        MathOp = MathOperation.NA;
        FormatString = "md8";
        ToStringUnsigned = true;
    }
}

