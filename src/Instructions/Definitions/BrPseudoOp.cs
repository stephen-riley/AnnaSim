namespace AnnaSim.Instructions.Definitions;

public partial class BrPseudoOp : InstructionDefinition
{
    public BrPseudoOp() : base()
    {
        Opcode = 0xff;
        Mnemonic = "br";
        OperandCount = 1;
        Type = InstructionType.Imm8;
        MathOp = MathOperation.NA;
        FormatString = "m8";
    }
}

