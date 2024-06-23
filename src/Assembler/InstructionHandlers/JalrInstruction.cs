using AnnaSim.Assembler;

namespace AnnaSim.Instructions.Definitions;

public partial class JalrInstruction
{
    protected override void AssembleImpl(params Operand[] operands)
    {
        MemoryImage[Addr++] = ToInstruction(operands);
    }

    public override Instruction ToInstruction(params Operand[] operands)
    {
        // jalr has two "modes" in assembly:
        //  1. a jump and link -- rs1 is specified as > 0
        //  2. a hard jump with no link -- rs1 is not specified and set to 0 by the assembler
        //
        // To support the second case, we might need to add a dummy register operand of r0
        var op1 = operands.Length > 1 ? operands[1] : Operand.Register("r0");

        return Instruction.NewRType(this, Asm.Register(operands[0]), Asm.Register(op1), 0x0);
    }
}

