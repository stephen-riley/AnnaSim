using AnnaSim.Assembler;

namespace AnnaSim.Instructions.Definitions;

public partial class SwInstruction
{
    protected override void AssembleImpl(params Operand[] operands)
    {
        MemoryImage[Addr++] = ToInstruction(operands);
    }

    public override Instruction ToInstruction(params Operand[] operands)
    {
        // TODO: figure out how to make its InstructionInfo have variable required operands
        return Instruction.NewImm6(this, Asm.Register(operands[0]), Asm.Register(operands[1]), (short)(operands.Length == 3 ? operands[2] : 0));
    }
}

