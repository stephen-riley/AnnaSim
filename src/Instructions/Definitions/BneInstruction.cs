using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class BneInstruction : AbstractInstruction
{
    public BneInstruction() : base()
    {
        Opcode = 11;
        Mnemonic = "bne";
        OperandCount = 2;
        Type = InstructionType.Imm8;
        MathOp = MathOperation.NA;
    }
}

