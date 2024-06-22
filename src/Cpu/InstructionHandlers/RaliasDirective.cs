namespace AnnaSim.Instructions.Definitions;

public partial class RaliasDirective
{
    protected override uint ExecuteImpl(Instruction instruction) => throw new NotImplementedException($"{instruction.Idef.Mnemonic}.{nameof(ExecuteImpl)}");
}

