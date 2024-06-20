using AnnaSim.Assember;
using AnnaSim.Cpu;

namespace AnnaSim.Instructions;

public class BneInstruction : AbstractInstruction
{
    public BneInstruction() : base()
    {
        Opcode = 11;
        Mnemonic = "bne";
        OperandCount = 2;
        Type = InstructionType.Imm8;
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

