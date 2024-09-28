using AnnaSim.AsmParsing;
using AnnaSim.Assembler;
using AnnaSim.Cpu.Memory;

namespace AnnaSim.Instructions.Definitions;

public partial class AddiInstruction
{
    protected override void AssembleImpl(CstInstruction ci) => Addr = ci.AssignBits(Addr, (Word)ToInstruction(ci.Operands));

    public override Instruction ToInstructionImpl(Operand[] operands)
    {
        return Instruction.NewImm6(this, Asm.Register(operands[0]), Asm.Register(operands[1]), (short)operands[2]);
    }
}

