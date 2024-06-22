namespace AnnaSim.Instructions.Definitions;

public partial class AddInstruction : InstructionDefinition
{
    public AddInstruction() : base()
    {
        Opcode = 0;
        Mnemonic = "add";
        OperandCount = 3;
        Type = InstructionType.R;
        MathOp = MathOperation.Add;
        FormatString = "md12";
    }
}

