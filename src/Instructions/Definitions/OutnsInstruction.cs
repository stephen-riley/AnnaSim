namespace AnnaSim.Instructions.Definitions;

public partial class OutnInstruction : InstructionDefinition
{
    public OutnInstruction() : base()
    {
        Opcode = 3;
        Mnemonic = "outn";
        OperandCount = 1;
        Type = InstructionType.R;
        MathOp = MathOperation.OutNumString;
        FormatString = "md";
    }
}

