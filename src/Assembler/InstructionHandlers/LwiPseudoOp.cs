using AnnaSim.AsmParsing;
using AnnaSim.Assembler;

namespace AnnaSim.Instructions.Definitions;

public partial class LwiPseudoOp
{
    protected override void AssembleImpl(CstInstruction ci)
    {
        var lliDef = ISA.GetIdef((ushort)(new LliInstruction().Opcode << 12));
        var luiDef = ISA.GetIdef((ushort)(new LuiInstruction().Opcode << 12));

        var operands = ci.Operands;

        Addr = ci.AssignBits(
            Addr,
            lliDef.ToInstruction(operands),
            luiDef.ToInstruction(operands)
        );
    }

    public override Instruction ToInstructionImpl(Operand[] operands) => throw new NotImplementedException($"{Mnemonic}.{nameof(ToInstructionImpl)}");
}
