using AnnaSim.Assember;
using AnnaSim.Cpu;

namespace AnnaSim.Instructions;

public class RaliasDirective : AbstractInstruction
{
    public RaliasDirective() : base()
    {
        Opcode = -1;
        Mnemonic = ".ralias";
        OperandCount = 2;
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

