using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class BgtInstruction : AbstractInstruction
{
    public BgtInstruction() : base()
    {
        Opcode = 12;
        Mnemonic = "bgt";
        OperandCount = 2;
        Type = InstructionType.Imm8;
        MathOp = MathOperation.NA;
    }
}

