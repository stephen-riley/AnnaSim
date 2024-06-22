namespace AnnaSim.Instructions.Definitions;

public partial class BleInstruction : InstructionDefinition
{
    public BleInstruction() : base()
    {
        Opcode = 15;
        Mnemonic = "ble";
        OperandCount = 2;
        Type = InstructionType.Imm8;
        MathOp = MathOperation.NA;
        FormatString = "md18";
    }
}

