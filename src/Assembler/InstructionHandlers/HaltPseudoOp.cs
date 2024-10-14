using AnnaSim.AsmParsing;
using AnnaSim.Assembler;
using AnnaSim.Cpu.Memory;

namespace AnnaSim.Instructions.Definitions;

public partial class HaltPseudoOp
{
    protected override void AssembleImpl(CstInstruction ci) => Addr = ci.AssignBits(Addr, (Word)ISA.Lookup["out"].ToInstruction([Operand.Register("r0")]));

    public override Instruction ToInstructionImpl(Operand[] operands) => throw new InvalidOperationException($"Cannot create instruction from directive {Mnemonic}");
}

