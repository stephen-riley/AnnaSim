using AnnaSim.Assember;
using AnnaSim.Cpu;

namespace AnnaSim.Instructions;

public class AndInstruction : AbstractInstruction
{
    public AndInstruction() : base()
    {
        Opcode = 0;
        Mnemonic = "and";
        OperandCount = 3;
        Type = InstructionType.R;
        MathOp = MathOperation.And;
    }

    public override void Assemble(AnnaAssembler asm)
    {
    }

    public override uint Execute(AnnaMachine cpu)
    {
        return 0xffff;
    }
}

