using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class BeqInstruction : AbstractInstruction
{
    public BeqInstruction() : base()
    {
        Opcode = 10;
        Mnemonic = "beq";
        OperandCount = 2;
        Type = InstructionType.Imm8;
        MathOp = MathOperation.NA;
    }
}

