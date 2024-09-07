using AnnaSim.Assembler;

namespace AnnaSim.Instructions.Definitions;

public partial class OutnsInstruction
{
    protected override void AssembleImpl(Operand[] operands, string? label)
    {
        MemoryImage[Addr] = ToInstruction(operands); Addr++;
    }

    public override Instruction ToInstructionImpl(Operand[] operands)
    {
        return Instruction.NewRType(this, Asm.Register(operands[0]), 0x0, 0x00);
    }
}

