namespace AnnaSim.Instructions.Definitions;

public partial class DivInstruction : InstructionDefinition
{
    public DivInstruction() : base()
    {
        Opcode = 0;
        Mnemonic = "div";
        OperandCount = 3;
        Type = InstructionType.R;
        MathOp = MathOperation.Div;
        FormatString = "md12";
    }
}

