using AnnaSim.Assember;
using AnnaSim.Cpu;

namespace AnnaSim.Instructions;

public class FillDirective : AbstractInstruction
{
    public FillDirective() : base()
    {
        Opcode = -1;
        Mnemonic = ".fill";
        OperandCount = -1;
        Type = InstructionType.Directive;
        MathOp = MathOperation.NA;
    }

    public override void Assemble(AnnaAssembler asm)
    {
    }

    public override uint Execute(AnnaMachine cpu)
    {
        return 0xffff;
    }
}

