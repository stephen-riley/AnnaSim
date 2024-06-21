using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class HaltDirective : AbstractInstruction
{
    public HaltDirective() : base()
    {
        Opcode = -1;
        Mnemonic = ".halt";
        OperandCount = 0;
        Type = InstructionType.Directive;
        MathOp = MathOperation.NA;
    }
}

