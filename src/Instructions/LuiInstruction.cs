using AnnaSim.Assember;
using AnnaSim.Cpu;

namespace AnnaSim.Instructions;

public class LuiInstruction : AbstractInstruction
{
    public LuiInstruction() : base()
    {
        Opcode = 9;
        Mnemonic = "lui";
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

