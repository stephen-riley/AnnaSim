using AnnaSim.Assember;
using AnnaSim.Cpu;

namespace AnnaSim.Instructions;

public class AddiInstruction : AbstractInstruction
{
    public AddiInstruction() : base()
    {
        Opcode = 4;
        Mnemonic = "addi";
        OperandCount = 3;
        Type = InstructionType.Imm6;
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

