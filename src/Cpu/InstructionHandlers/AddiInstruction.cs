using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class AddiInstruction
{
    public override uint Execute(AnnaMachine cpu, params string[] operands)
    {
        throw new NotImplementedException($"AddiInstruction.{nameof(Execute)}");
    }
}

