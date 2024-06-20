using AnnaSim.Assember;
using AnnaSim.Cpu;

namespace AnnaSim.Instructions;

public class AddInstruction : AbstractInstruction
{
    public AddInstruction() : base()
    {
        Opcode = 0;
        Mnemonic = "add";
        OperandCount = 3;
        Type = InstructionType.R;
        MathOp = MathOperation.Add;
    }

    public override void Assemble(AnnaAssembler asm)
    {
    }

    public override uint Execute(AnnaMachine cpu)
    {
        return 0xffff;
    }
}

