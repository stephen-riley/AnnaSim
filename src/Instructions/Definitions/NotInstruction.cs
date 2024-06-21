using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class NotInstruction : AbstractInstruction
{
    public NotInstruction() : base()
    {
        Opcode = 0;
        Mnemonic = "not";
        OperandCount = 2;
        Type = InstructionType.R;
        MathOp = MathOperation.Not;
    }
}

