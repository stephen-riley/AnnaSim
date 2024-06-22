using AnnaSim.Assembler;

namespace AnnaSim.Instructions.Definitions;

public partial class OrInstruction
{
    protected override void AssembleImpl(params Operand[] operands)
    {
        MemoryImage[Addr++] = ToInstruction(operands);
    }

    public override Instruction ToInstruction(params Operand[] operands)
    {
        return Instruction.NewRType(this, (ushort)operands[0], (ushort)operands[1], (ushort)operands[2]);
    }
}

