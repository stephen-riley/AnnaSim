using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class RaliasDirective : AbstractInstruction
{
    public RaliasDirective() : base()
    {
        Opcode = -1;
        Mnemonic = ".ralias";
        OperandCount = 2;
        Type = InstructionType.Directive;
        MathOp = MathOperation.NA;
    }
}

