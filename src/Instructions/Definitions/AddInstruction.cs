using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class AddInstruction : AbstractInstruction
{
    public AddInstruction() : base()
    {
        Opcode = 0;
        Mnemonic = "add";
        OperandCount = 3;
        Type = InstructionType.R;
        MathOp = MathOperation.Add;
    }
}

