using AnnaSim.AsmParsing;
using AnnaSim.Assembler;
using AnnaSim.Cpu.Memory;

namespace AnnaSim.Instructions.Definitions;

public partial class OrgDirective
{
    protected override void AssembleImpl(CstInstruction ci)
    {
        if (ci.Labels.Count > 0)
        {
            throw new InvalidOperationException(".org cannot have a label associated");
        }

        Asm.Addr = (uint)ci.Operands[0];
    }

    public override Instruction ToInstructionImpl(Operand[] operands) => throw new InvalidOperationException($"Cannot create instruction from directive {Mnemonic}");
}

