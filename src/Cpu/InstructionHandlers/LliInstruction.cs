using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class LliInstruction
{
    public override uint Execute(AnnaMachine cpu, params string[] operands)
    {
        throw new NotImplementedException($"LliInstruction.{nameof(Execute)}");
    }
}

