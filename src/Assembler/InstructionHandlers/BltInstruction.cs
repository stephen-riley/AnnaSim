using AnnaSim.AsmParsing;
using AnnaSim.Assembler;
using AnnaSim.Cpu.Memory;

namespace AnnaSim.Instructions.Definitions;

public partial class BltInstruction
{
    protected override void AssembleImpl(Operand[] operands, string? label)
    {
        MemoryImage[Addr] = ToInstruction(operands); Addr++;
    }

    protected override void AssembleImpl(CstInstruction ci) => Addr = ci.AssignBits(Addr, (Word)ToInstruction(ci.Operands));

    public override Instruction ToInstructionImpl(Operand[] operands)
    {
        return Instruction.NewImm8(this, Asm.Register(operands[0]), (short)operands[1]);
    }
}

