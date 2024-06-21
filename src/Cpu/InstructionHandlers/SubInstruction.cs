using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class SubInstruction
{
    public override uint Execute(AnnaMachine cpu, params string[] operands)
    {
        throw new NotImplementedException($"SubInstruction.{nameof(Execute)}");
    }
}

