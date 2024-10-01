using AnnaSim.AsmParsing;
using AnnaSim.Assembler;

namespace AnnaSim.Instructions.Definitions;

public partial class PushPseudoOp
{
    protected override void AssembleImpl(CstInstruction ci)
    {
        var swDef = ISA.GetIdef((ushort)(new SwInstruction().Opcode << 12));
        var addiDef = ISA.GetIdef((ushort)(new AddiInstruction().Opcode << 12));

        // operand[0] is the stack pointer
        // operand[1] is the base address (we hardcode 0 as the offset)

        var operands = ci.Operands;

        Addr = ci.AssignBits(
            Addr,
            swDef.ToInstruction([operands[1], operands[0], 0]),
            addiDef.ToInstruction([operands[0], operands[0], -1])
        );
    }

    public override Instruction ToInstructionImpl(Operand[] operands) => throw new NotImplementedException($"{Mnemonic}.{nameof(ToInstructionImpl)}");
}

