using AnnaSim.AsmParsing;
using AnnaSim.Assembler;
using AnnaSim.Cpu.Memory;

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

    protected override void AssembleImpl(CstInstruction ci)
    {
        if (ci.Labels.Count == 0)
        {
            throw new InvalidOperationException($".def must have a label");
        }

        if (ci.Operand1 is not null)
        {
            var dest = Asm.ParseOperand(ci.Operand1).AsUInt();
            foreach (var label in ci.Labels)
            {
                Asm.labels[label] = dest;
            }
        }
    }

    public override Instruction ToInstructionImpl(Operand[] operands) => throw new InvalidOperationException($"Cannot create instruction from directive {Mnemonic}");
}

