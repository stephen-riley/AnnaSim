using System.Text.RegularExpressions;
using AnnaSim.AsmParsing;
using AnnaSim.Assembler;
using AnnaSim.Cpu.Memory;

namespace AnnaSim.Instructions.Definitions;

public partial class CstrDirective
{
    protected override void AssembleImpl(CstInstruction ci)
    {
        var operands = ci.Operands;
        if (operands[0].Type != OperandType.String)
        {
            throw new InvalidOperationException($"operand type {operands[0].Type} is {nameof(OperandType.String)}");
        }


        var str = Regex.Unescape(operands[0].Str);
        var words = str.Select(c => (Word)c).ToArray();

        Addr = ci.AssignBits(Addr, [.. words, 0]);
    }

    public override Instruction ToInstructionImpl(Operand[] operands) => throw new InvalidOperationException($"Cannot create instruction from directive {Mnemonic}");
}

