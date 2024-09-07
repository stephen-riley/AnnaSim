namespace AnnaSim.Instructions.Definitions;

public partial class OutsInstruction : InstructionDefinition
{
    public OutsInstruction() : base()
    {
        Opcode = 3;
        Mnemonic = "outs";
        OperandCount = 1;
        Type = InstructionType.R;
        MathOp = MathOperation.OutString;
        FormatString = "md";
    }
}

