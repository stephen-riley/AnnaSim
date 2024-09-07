namespace AnnaSim.Instructions.Definitions;

public partial class OutnsInstruction : InstructionDefinition
{
    public OutnsInstruction() : base()
    {
        Opcode = 3;
        Mnemonic = "outns";
        OperandCount = 1;
        Type = InstructionType.R;
        MathOp = MathOperation.OutNumString;
        FormatString = "md";
    }
}

