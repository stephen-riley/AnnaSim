using System.Diagnostics;
using AnnaSim.Assembler;

namespace AnnaSim.Instructions.Definitions;

public partial class CstrDirective
{
    protected override void AssembleImpl(Operand[] operands, string? label)
    {
        if (!(operands[0].Str.StartsWith('"') && operands[0].Str.EndsWith('"')))
        {
            throw new InvalidOperationException($"Operand.Str {operands[0].Str} does not start and end with double quotes");
        }

        foreach (var c in operands[0].Str[1..^1])
        {
            MemoryImage[Addr++] = c;
        }

        MemoryImage[Addr++] = 0;
    }

    public override Instruction ToInstruction(Operand[] operands) => throw new InvalidOperationException($"Cannot create instruction from directive {Mnemonic}");
}

