using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class BneInstruction
{
    public override uint Execute(AnnaMachine cpu, params string[] operands)
    {
        throw new NotImplementedException($"BneInstruction.{nameof(Execute)}");
    }
}

