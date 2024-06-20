using AnnaSim.Assember;
using AnnaSim.Cpu;

namespace AnnaSim.Instructions;

public class SubInstruction : AbstractInstruction
{
    public SubInstruction() : base()
    {
        Opcode = 0;
        Mnemonic = "sub";
        OperandCount = 3;
        Type = InstructionType.R;
        MathOp = MathOperation.Sub;
    }

    public override void Assemble(AnnaAssembler asm)
    {
    }

    public override uint Execute(AnnaMachine cpu)
    {
        return 0xffff;
    }
}

