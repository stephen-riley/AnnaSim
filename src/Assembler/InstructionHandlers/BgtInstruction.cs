using AnnaSim.Assembler;

namespace AnnaSim.Instructions.Definitions;

public partial class BgtInstruction
{
    protected override void AssembleImpl(Operand[] operands, string? label)
    {
        MemoryImage[Addr] = ToInstruction(operands); Addr++;
    }

    public override Instruction ToInstructionImpl(Operand[] operands)
    {
        return Instruction.NewImm8(this, Asm.Register(operands[0]), (short)operands[1]);
    }
}

