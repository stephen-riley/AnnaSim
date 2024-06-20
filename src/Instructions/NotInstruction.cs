using AnnaSim.Assember;
using AnnaSim.Cpu;

namespace AnnaSim.Instructions;

public class NotInstruction : AbstractInstruction
{
    public NotInstruction() : base()
    {
        Opcode = 0;
        Mnemonic = "not";
        OperandCount = 2;
        Type = InstructionType.R;
        MathOp = MathOperation.Not;
    }

    public override void Assemble(AnnaAssembler asm)
    {
    }

    public override uint Execute(AnnaMachine cpu)
    {
        return 0xffff;
    }
}

