using AnnaSim.AsmParsing;
using AnnaSim.Assembler;

namespace AnnaSim.Instructions.Definitions;

public partial class RaliasDirective
{
    protected override void AssembleImpl(CstInstruction ci)
    {
        if (ci.Labels.Count > 0)
        {
            throw new InvalidOperationException(".ralias cannot have a label associated");
        }

        if (ci.Operand1 is null || ci.Operand2 is null)
        {
            throw new InvalidOperationException(".ralias must have two operands");
        }

        if (ci.Operand1.StartsWith('r') && ci.Operand2.StartsWith('r'))
        {
            Asm.registerAliases[ci.Operand1] = ci.Operand2;
        }
        else
        {
            throw new InvalidOperationException("in .ralias, all aliases must start with r");
        }
    }

    public override Instruction ToInstructionImpl(Operand[] operands) => throw new InvalidOperationException($"Cannot create instruction from directive {Mnemonic}");
}

