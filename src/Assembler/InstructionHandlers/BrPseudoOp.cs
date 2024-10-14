using AnnaSim.AsmParsing;
using AnnaSim.Assembler;
using AnnaSim.Cpu.Memory;

namespace AnnaSim.Instructions.Definitions;

public partial class BrPseudoOp
{
    protected override void AssembleImpl(CstInstruction ci)
    {
        var beqDef = ISA.GetIdef((ushort)(new BeqInstruction().Opcode << 12));
        var operands = ci.Operands;
        Addr = ci.AssignBits(Addr, (Word)beqDef.ToInstruction([Operand.Register("r0"), operands[0]]));
    }

    public override Instruction ToInstructionImpl(Operand[] operands) => throw new NotImplementedException($"{Mnemonic}.{nameof(ToInstructionImpl)}");
}

