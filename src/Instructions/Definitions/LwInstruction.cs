using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class LwInstruction : AbstractInstruction
{
    public LwInstruction() : base()
    {
        Opcode = 6;
        Mnemonic = "lw";
        OperandCount = 3;
        Type = InstructionType.Imm6;
        MathOp = MathOperation.NA;
    }
}

