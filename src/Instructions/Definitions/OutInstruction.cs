using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class OutInstruction : AbstractInstruction
{
    public OutInstruction() : base()
    {
        Opcode = 3;
        Mnemonic = "out";
        OperandCount = 1;
        Type = InstructionType.R;
        MathOp = MathOperation.NA;
    }
}

