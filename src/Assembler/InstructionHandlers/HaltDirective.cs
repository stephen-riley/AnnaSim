using AnnaSim.Assembler;

namespace AnnaSim.Instructions.Definitions;

public partial class HaltDirective
{
    protected override void AssembleImpl(params Operand[] operands)
    {
        MemoryImage[Addr++] = I.Lookup["out"].ToInstruction(rd: 0);
    }

    public override Instruction ToInstruction(params Operand[] operands) => throw new InvalidOperationException($"Cannot create instruction from directive {Mnemonic}");
}

