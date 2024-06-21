using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class LwInstruction
{
    public override uint Execute(AnnaMachine cpu, params string[] operands)
    {
        throw new NotImplementedException($"LwInstruction.{nameof(Execute)}");
    }
}

