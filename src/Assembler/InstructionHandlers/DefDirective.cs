using AnnaSim.Assembler;

namespace AnnaSim.Instructions.Definitions;

public partial class DefDirective
{
    protected override void AssembleImpl(Operand[] operands, string? label)
    {
        if (label is null)
        {
            throw new InvalidOperationException($".def must have a label");
        }

        Asm.labels[label] = operands[0].AsUInt();
    }

    public override Instruction ToInstructionImpl(Operand[] operands) => throw new InvalidOperationException($"Cannot create instruction from directive {Mnemonic}");
}

