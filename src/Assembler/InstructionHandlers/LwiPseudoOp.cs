using AnnaSim.AsmParsing;
using AnnaSim.Assembler;
using AnnaSim.Cpu.Memory;

namespace AnnaSim.Instructions.Definitions;

public partial class LwiPseudoOp
{
    protected override void AssembleImpl(Operand[] operands, string? label)
    {
        var lliDef = ISA.GetIdef((ushort)(new LliInstruction().Opcode << 12));
        var luiDef = ISA.GetIdef((ushort)(new LuiInstruction().Opcode << 12));

        MemoryImage[Addr] = lliDef.ToInstruction(operands); Addr++;
        MemoryImage[Addr] = luiDef.ToInstruction(operands); Addr++;
    }

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
