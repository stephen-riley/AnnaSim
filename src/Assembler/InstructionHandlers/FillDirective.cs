using AnnaSim.AsmParsing;
using AnnaSim.Assembler;
using AnnaSim.Cpu.Memory;

namespace AnnaSim.Instructions.Definitions;

public partial class FillDirective
{
    protected override void AssembleImpl(CstInstruction ci)
    {
        Addr = ci.AssignBits(Addr, ci.Operands.Select(o => (Word)o.AsUInt()).ToArray());
    }

    public override Instruction ToInstructionImpl(Operand[] operands) => throw new InvalidOperationException($"Cannot create instruction from directive {Mnemonic}");
}

