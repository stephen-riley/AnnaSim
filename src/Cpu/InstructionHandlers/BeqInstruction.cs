using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class BeqInstruction
{
    public override uint Execute(AnnaMachine cpu, params string[] operands)
    {
        throw new NotImplementedException($"BeqInstruction.{nameof(Execute)}");
    }
}

