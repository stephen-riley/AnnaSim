using AnnaSim.Assember;
using AnnaSim.Cpu;

namespace AnnaSim.Instructions;

public class JalrInstruction : AbstractInstruction
{
    public JalrInstruction() : base()
    {
        Opcode = 1;
        Mnemonic = "jalr";
        OperandCount = 2;
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

