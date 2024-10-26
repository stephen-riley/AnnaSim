using System.Text.RegularExpressions;
using AnnaSim.AsmParsing;
using AnnaSim.Assembler;
using AnnaSim.Cpu.Memory;

namespace AnnaSim.Instructions.Definitions;

public partial class FrameDirective
{
    protected override void AssembleImpl(CstInstruction ci)
    {
        var operands = ci.Operands;
        if (operands[0].Type != OperandType.String)
        {
            throw new InvalidOperationException($"operand type {operands[0].Type} is {nameof(OperandType.String)}");
        }

        if (operands[0].Str == "on")
        {
            Asm.TmpFrameDef.StartAddr = Addr;
        }
        else if (operands[0].Str == "off")
        {
            Asm.TmpFrameDef.EndAddr = Addr;
            Asm.FrameDefs[Asm.TmpFrameDef.Name] = Asm.TmpFrameDef;
        }
        else
        {
            BuildFrameDef(operands[0].Str);
        }
    }

    public override Instruction ToInstructionImpl(Operand[] operands) => throw new InvalidOperationException($"Cannot create instruction from directive {Mnemonic}");

    private void BuildFrameDef(string def)
    {
        var pieces = def[1..].Split('|');
        Asm.TmpFrameDef = new FrameDef(pieces[0]);

        foreach (var p in pieces[1..])
        {
            var keyVal = p.Split('~');
            var index = int.Parse(keyVal[0]);
            Asm.TmpFrameDef.AddMember(index, keyVal[1]);
        }
    }
}

