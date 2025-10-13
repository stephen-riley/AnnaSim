namespace AnnaSim.Instructions.Definitions;

public partial class OutcInstruction : InstructionDefinition
{
    public OutcInstruction() : base()
    {
        Opcode = 3;
        Mnemonic = "outc";
        OperandCount = 1;
        Type = InstructionType.R;
        MathOp = MathOperation.OutCharString;
        FormatString = "md";
    }
}

