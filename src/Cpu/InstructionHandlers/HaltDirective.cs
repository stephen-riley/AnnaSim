using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class HaltDirective
{
    public override uint Execute(AnnaMachine cpu, params string[] operands)
    {
        throw new NotImplementedException($"HaltDirective.{nameof(Execute)}");
    }
}

