using AnnaSim.Assembler;

namespace AnnaSim.Instructions.Definitions;

public partial class AndInstruction
{
    protected override void AssembleImpl(params Operand[] operands)
    {
        MemoryImage[Addr++] = ToInstruction(operands);
    }

    public override Instruction ToInstruction(params Operand[] operands)
    {
        return Instruction.NewRType(this, Asm.Register(operands[0]), Asm.Register(operands[1]), Asm.Register(operands[2]));
    }
}

