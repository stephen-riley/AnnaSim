using AnnaSim.Assembler;

namespace AnnaSim.Instructions.Definitions;

public partial class AddInstruction
{
    protected override void AssembleImpl(Operand[] operands, string? label)
    {
        MemoryImage[Addr++] = ToInstruction(operands);
    }

    public override Instruction ToInstruction(Operand[] operands)
    {
        return Instruction.NewRType(this, Asm.Register(operands[0]), Asm.Register(operands[1]), Asm.Register(operands[2]));
    }
}

