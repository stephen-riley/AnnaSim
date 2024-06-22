namespace AnnaSim.Instructions.Definitions;

public partial class LuiInstruction : InstructionDefinition
{
    public LuiInstruction() : base()
    {
        Opcode = 9;
        Mnemonic = "lui";
        OperandCount = 2;
        Type = InstructionType.Imm8;
        MathOp = MathOperation.NA;
        FormatString = "md18";
    }
}

