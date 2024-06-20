using AnnaSim.Assember;
using AnnaSim.Cpu;

namespace AnnaSim.Instructions;

public class OrInstruction : AbstractInstruction
{
    public OrInstruction() : base()
    {
        Opcode = 0;
        Mnemonic = "or";
        OperandCount = 3;
        Type = InstructionType.R;
        MathOp = MathOperation.Or;
    }

    public override void Assemble(AnnaAssembler asm)
    {
    }

    public override uint Execute(AnnaMachine cpu)
    {
        return 0xffff;
    }
}

