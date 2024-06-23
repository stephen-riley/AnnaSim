namespace AnnaSim.Instructions.Definitions;

public partial class BeqInstruction : InstructionDefinition
{
    public BeqInstruction() : base()
    {
        Opcode = 10;
        Mnemonic = "beq";
        OperandCount = 2;
        Type = InstructionType.Imm8;
        MathOp = MathOperation.NA;
        FormatString = "md8";
    }
}

