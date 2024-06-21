using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class FillDirective
{
    public override uint Execute(AnnaMachine cpu, params string[] operands)
    {
        throw new NotImplementedException($"FillDirective.{nameof(Execute)}");
    }
}

