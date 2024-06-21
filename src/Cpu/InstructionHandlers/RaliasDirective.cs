using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class RaliasDirective
{
    public override uint Execute(AnnaMachine cpu, params string[] operands)
    {
        throw new NotImplementedException($"RaliasDirective.{nameof(Execute)}");
    }
}

