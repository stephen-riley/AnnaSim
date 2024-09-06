using AnnaSim.Assembler;

namespace AnnaSim.Instructions.Definitions;

public partial class SwInstruction
{
    protected override void AssembleImpl(Operand[] operands, string? label)
    {
        MemoryImage[Addr] = ToInstruction(operands); Addr++;
    }

    public override Instruction ToInstructionImpl(Operand[] operands)
    {
        // TODO: figure out how to make its InstructionInfo have variable required operands
        return Instruction.NewImm6(this, Asm.Register(operands[0]), Asm.Register(operands[1]), (short)(operands.Length == 3 ? operands[2] : 0));
    }
}

