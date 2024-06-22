namespace AnnaSim.Instructions.Definitions;

public partial class FillDirective
{
    protected override uint ExecuteImpl(Instruction instruction) => throw new NotImplementedException($"{instruction.Idef.Mnemonic}.{nameof(ExecuteImpl)}");
}

