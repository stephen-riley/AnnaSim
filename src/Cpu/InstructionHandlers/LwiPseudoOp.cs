namespace AnnaSim.Instructions.Definitions;

public partial class LwiPseudoOp
{
    protected override uint ExecuteImpl(Instruction instruction) => throw new NotImplementedException($"{instruction.Idef.Mnemonic}.{nameof(ExecuteImpl)}");
}

