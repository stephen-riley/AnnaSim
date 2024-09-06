using AnnaSim.Assembler;

namespace AnnaSim.Instructions.Definitions;

public partial class FillDirective
{
    protected override void AssembleImpl(Operand[] operands, string? label)
    {
        foreach (var operand in operands)
        {
            MemoryImage[Addr] = operand.AsUInt(); Addr++;
        }
    }

    public override Instruction ToInstructionImpl(Operand[] operands) => throw new InvalidOperationException($"Cannot create instruction from directive {Mnemonic}");
}

