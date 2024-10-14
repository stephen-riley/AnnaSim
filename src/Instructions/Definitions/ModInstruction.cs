namespace AnnaSim.Instructions.Definitions;

public partial class ModInstruction : InstructionDefinition
{
    public ModInstruction() : base()
    {
        Opcode = 0;
        Mnemonic = "mod";
        OperandCount = 3;
        Type = InstructionType.R;
        MathOp = MathOperation.Mod;
        FormatString = "md12";
    }
}

