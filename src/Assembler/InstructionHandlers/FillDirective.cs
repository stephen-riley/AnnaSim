using AnnaSim.Assembler;

namespace AnnaSim.Instructions.Definitions;

public partial class FillDirective
{
    protected override void AssembleImpl(params Operand[] operands)
    {
        foreach (var operand in operands)
        {
            MemoryImage[Addr++] = (uint)operand;
        }
    }

    public override Instruction ToInstruction(params Operand[] operands) => throw new InvalidOperationException($"Cannot create instruction from directive {Mnemonic}");
}

