namespace AnnaSim.Instructions.Definitions;

public partial class HaltPseudoOp
{
    protected override uint ExecuteImpl(Instruction instruction) => throw new NotImplementedException($"{instruction.Idef.Mnemonic}.{nameof(ExecuteImpl)}");
}

