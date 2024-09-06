using AnnaSim.Assembler;
using AnnaSim.Exceptions;

namespace AnnaSim.Instructions.Definitions;

public partial class RaliasDirective
{
    protected override void AssembleImpl(Operand[] operands, string? label)
    {
        if (operands[0].IsStandardRegisterName() && ((string)operands[1]).StartsWith('r'))
        {
            Asm.registerAliases[(string)operands[1]] = (string)operands[0];
        }
        else
        {
            throw new InvalidOpcodeException($"cannot parse directive {Mnemonic} {string.Join(' ', (IEnumerable<Operand>)operands)}");
        }
    }

    public override Instruction ToInstructionImpl(Operand[] operands) => throw new InvalidOperationException($"Cannot create instruction from directive {Mnemonic}");
}

