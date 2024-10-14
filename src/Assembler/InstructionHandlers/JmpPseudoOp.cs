using AnnaSim.AsmParsing;
using AnnaSim.Assembler;
using AnnaSim.Cpu.Memory;

namespace AnnaSim.Instructions.Definitions;

public partial class JmpPseudoOp
{
    protected override void AssembleImpl(CstInstruction ci)
    {
        var jalrDef = ISA.GetIdef((ushort)(new JalrInstruction().Opcode << 12));
        var operands = ci.Operands;
        Addr = ci.AssignBits(Addr, (Word)jalrDef.ToInstruction([operands[0], Operand.Register("r0"), Operand.Register("r0")]));
    }

    public override Instruction ToInstructionImpl(Operand[] operands) => throw new NotImplementedException($"{Mnemonic}.{nameof(ToInstructionImpl)}");
}

