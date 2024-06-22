using AnnaSim.Assembler;

namespace AnnaSim.Instructions.Definitions;

public partial class ShfInstruction
{
    protected override void AssembleImpl(params Operand[] operands)
    {
        MemoryImage[Addr++] = ToInstruction(operands);
    }

    public override Instruction ToInstruction(params Operand[] operands)
    {
        return Instruction.NewImm6(this, (ushort)operands[0], (ushort)operands[1], (short)operands[2]);
    }
}

