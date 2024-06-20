using AnnaSim.Assember;
using AnnaSim.Cpu;

namespace AnnaSim.Instructions;

public class InInstruction : AbstractInstruction
{
    public InInstruction() : base()
    {
        Opcode = 2;
        Mnemonic = "in";
        OperandCount = 1;
        Type = InstructionType.R;
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

