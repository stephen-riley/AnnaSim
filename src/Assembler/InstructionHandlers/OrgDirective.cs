using AnnaSim.AsmParsing;
using AnnaSim.Assembler;
using AnnaSim.Cpu.Memory;

namespace AnnaSim.Instructions.Definitions;

public partial class OrgDirective
{
    protected override void AssembleImpl(Operand[] operands, string? label)
    {
        if (label is not null)
        {
            throw new InvalidOperationException(".org cannot have a label associated");
        }

        Asm.Addr = (uint)(int)operands[0];
    }

    protected override void AssembleImpl(CstInstruction ci) => Asm.Addr = (uint)ci.Operands[0];

    public override Instruction ToInstructionImpl(Operand[] operands) => throw new InvalidOperationException($"Cannot create instruction from directive {Mnemonic}");
}

