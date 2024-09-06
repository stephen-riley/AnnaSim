using AnnaSim.Assembler;

namespace AnnaSim.Instructions.Definitions;

public partial class PushPseudoOp
{
    protected override void AssembleImpl(Operand[] operands, string? label)
    {
        var swDef = ISA.GetIdef((ushort)(new SwInstruction().Opcode << 12));
        var addiDef = ISA.GetIdef((ushort)(new AddiInstruction().Opcode << 12));

        MemoryImage[Addr] = swDef.ToInstruction([.. operands, 0]); Addr++;
        MemoryImage[Addr] = addiDef.ToInstruction([operands[0], operands[0], -1]); Addr++;
    }

    public override Instruction ToInstructionImpl(Operand[] operands) => throw new NotImplementedException($"{Mnemonic}.{nameof(ExecuteImpl)}");
}

