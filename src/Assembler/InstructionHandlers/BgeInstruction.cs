using AnnaSim.Assembler;

namespace AnnaSim.Instructions.Definitions;

public partial class BgeInstruction
{
    protected override void AssembleImpl(params Operand[] operands)
    {
        MemoryImage[Addr++] = ToInstruction(operands);
    }

    public override Instruction ToInstruction(params Operand[] operands)
    {
        return Instruction.NewImm8(this, Asm.Register(operands[0]), (short)operands[1]);
    }
}

