using AnnaSim.Assembler;

namespace AnnaSim.Instructions.Definitions;

public partial class LliInstruction
{
    protected override void AssembleImpl(params Operand[] operands)
    {
        MemoryImage[Addr++] = ToInstruction(operands);
    }

    public override Instruction ToInstruction(params Operand[] operands)
    {
        return Instruction.NewImm8(this, (ushort)operands[0], (short)operands[1]);
    }
}

