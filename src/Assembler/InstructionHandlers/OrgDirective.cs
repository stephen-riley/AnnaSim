using AnnaSim.Assembler;

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

    public override Instruction ToInstruction(Operand[] operands) => throw new InvalidOperationException($"Cannot create instruction from directive {Mnemonic}");
}

