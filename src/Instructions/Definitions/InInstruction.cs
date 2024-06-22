namespace AnnaSim.Instructions.Definitions;

public partial class InInstruction : InstructionDefinition
{
    public InInstruction() : base()
    {
        Opcode = 2;
        Mnemonic = "in";
        OperandCount = 1;
        Type = InstructionType.R;
        MathOp = MathOperation.NA;
        FormatString = "md";
    }
}

