using AnnaSim.Assembler;

namespace AnnaSim.Instructions.Definitions;

public partial class MovPseudoOp
{
    protected override void AssembleImpl(Operand[] operands, string? label)
    {
        var addDef = ISA.GetIdef((ushort)(new AddInstruction().Opcode << 12));

        MemoryImage[Addr] = addDef.ToInstruction([operands[0], operands[1], Operand.Register("r0")]); Addr++;
    }

    public override Instruction ToInstructionImpl(Operand[] operands) => throw new NotImplementedException($"{Mnemonic}.{nameof(ExecuteImpl)}");
}

