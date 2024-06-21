using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class AddiInstruction : AbstractInstruction
{
    public AddiInstruction() : base()
    {
        Opcode = 4;
        Mnemonic = "addi";
        OperandCount = 3;
        Type = InstructionType.Imm6;
        MathOp = MathOperation.NA;
    }
}

