using AnnaSim.Assember;
using AnnaSim.Cpu;

namespace AnnaSim.Instructions;

public class OutInstruction : AbstractInstruction
{
    public OutInstruction() : base()
    {
        Opcode = 3;
        Mnemonic = "out";
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

