using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class JalrInstruction : AbstractInstruction
{
    public JalrInstruction() : base()
    {
        Opcode = 1;
        Mnemonic = "jalr";
        OperandCount = 2;
        Type = InstructionType.R;
        MathOp = MathOperation.NA;
    }
}

