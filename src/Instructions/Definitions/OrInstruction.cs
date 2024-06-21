using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class OrInstruction : AbstractInstruction
{
    public OrInstruction() : base()
    {
        Opcode = 0;
        Mnemonic = "or";
        OperandCount = 3;
        Type = InstructionType.R;
        MathOp = MathOperation.Or;
    }
}

