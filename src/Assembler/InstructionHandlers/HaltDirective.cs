using AnnaSim.Assembler;

namespace AnnaSim.Instructions.Definitions;

public partial class HaltDirective
{
    protected override void AssembleImpl(Operand[] operands, string? label)
    {
        MemoryImage[Addr++] = ISA.Lookup["out"].ToInstruction(rd: 0);
    }

    public override Instruction ToInstruction(Operand[] operands) => throw new InvalidOperationException($"Cannot create instruction from directive {Mnemonic}");
}

