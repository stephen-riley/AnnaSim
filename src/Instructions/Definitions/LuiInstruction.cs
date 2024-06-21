using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class LuiInstruction : AbstractInstruction
{
    public LuiInstruction() : base()
    {
        Opcode = 9;
        Mnemonic = "lui";
        OperandCount = 2;
        Type = InstructionType.Imm8;
        MathOp = MathOperation.NA;
    }
}

