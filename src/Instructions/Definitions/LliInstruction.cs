using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class LliInstruction : AbstractInstruction
{
    public LliInstruction() : base()
    {
        Opcode = 8;
        Mnemonic = "lli";
        OperandCount = 2;
        Type = InstructionType.Imm8;
        MathOp = MathOperation.NA;
    }
}

