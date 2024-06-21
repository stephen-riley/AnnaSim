using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class FillDirective : AbstractInstruction
{
    public FillDirective() : base()
    {
        Opcode = -1;
        Mnemonic = ".fill";
        OperandCount = -1;
        Type = InstructionType.Directive;
        MathOp = MathOperation.NA;
    }
}

