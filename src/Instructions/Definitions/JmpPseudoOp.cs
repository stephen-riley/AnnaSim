namespace AnnaSim.Instructions.Definitions;

public partial class JmpPseudoOp : InstructionDefinition
{
    public JmpPseudoOp() : base()
    {
        Opcode = 0xff;
        Mnemonic = "jmp";
        OperandCount = 1;
        Type = InstructionType.R;
        MathOp = MathOperation.NA;
        FormatString = "mI";
    }
}

