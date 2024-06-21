using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class SwInstruction
{
    public override uint Execute(AnnaMachine cpu, params string[] operands)
    {
        throw new NotImplementedException($"SwInstruction.{nameof(Execute)}");
    }
}

