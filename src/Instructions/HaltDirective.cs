using AnnaSim.Assember;
using AnnaSim.Cpu;

namespace AnnaSim.Instructions;

public class HaltDirective : AbstractInstruction
{
    public HaltDirective() : base()
    {
        Opcode = -1;
        Mnemonic = ".halt";
        OperandCount = 0;
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

