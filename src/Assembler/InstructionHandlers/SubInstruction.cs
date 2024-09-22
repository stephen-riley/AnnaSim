using AnnaSim.AsmParsing;
using AnnaSim.Assembler;
using AnnaSim.Cpu.Memory;

namespace AnnaSim.Instructions.Definitions;

public partial class SubInstruction
{
    protected override void AssembleImpl(Operand[] operands, string? label)
    {
        MemoryImage[Addr] = ToInstruction(operands); Addr++;
    }

    protected override void AssembleImpl(CstInstruction ci) => Addr = ci.AssignBits(Addr, (Word)ToInstruction(ci.Operands));

    public override Instruction ToInstructionImpl(Operand[] operands)
    {
        return Instruction.NewRType(this, Asm.Register(operands[0]), Asm.Register(operands[1]), Asm.Register(operands[2]));
    }
}

