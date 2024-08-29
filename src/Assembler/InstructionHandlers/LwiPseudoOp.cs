using AnnaSim.Assembler;

namespace AnnaSim.Instructions.Definitions;

public partial class LwiPseudoOp
{
    protected override void AssembleImpl(Operand[] operands, string? label)
    {
        var lliDef = ISA.GetIdef((ushort)(new LliInstruction().Opcode << 12));
        var luiDef = ISA.GetIdef((ushort)(new LuiInstruction().Opcode << 12));

        MemoryImage[Addr++] = lliDef.ToInstruction(operands);
        MemoryImage[Addr++] = luiDef.ToInstruction(operands);
    }

    public override Instruction ToInstruction(Operand[] operands) => throw new NotImplementedException($"{Mnemonic}.{nameof(ExecuteImpl)}");
}

