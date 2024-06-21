using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class ShfInstruction : AbstractInstruction
{
    public ShfInstruction() : base()
    {
        Opcode = 5;
        Mnemonic = "shf";
        OperandCount = 3;
        Type = InstructionType.Imm6;
        MathOp = MathOperation.NA;
    }
}

