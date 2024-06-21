using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class BltInstruction : AbstractInstruction
{
    public BltInstruction() : base()
    {
        Opcode = 14;
        Mnemonic = "blt";
        OperandCount = 2;
        Type = InstructionType.Imm8;
        MathOp = MathOperation.NA;
    }
}

