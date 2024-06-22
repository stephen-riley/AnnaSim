namespace AnnaSim.Instructions.Definitions;

public partial class SubInstruction : InstructionDefinition
{
    public SubInstruction() : base()
    {
        Opcode = 0;
        Mnemonic = "sub";
        OperandCount = 3;
        Type = InstructionType.R;
        MathOp = MathOperation.Sub;
        FormatString = "md12";
    }
}

