using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class SubInstruction : AbstractInstruction
{
    public SubInstruction() : base()
    {
        Opcode = 0;
        Mnemonic = "sub";
        OperandCount = 3;
        Type = InstructionType.R;
        MathOp = MathOperation.Sub;
    }
}

