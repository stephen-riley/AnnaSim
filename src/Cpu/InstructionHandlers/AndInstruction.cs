using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class AndInstruction
{
    public override uint Execute(AnnaMachine cpu, params string[] operands)
    {
        throw new NotImplementedException($"AndInstruction.{nameof(Execute)}");
    }
}

