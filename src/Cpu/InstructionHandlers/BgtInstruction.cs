using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class BgtInstruction
{
    public override uint Execute(AnnaMachine cpu, params string[] operands)
    {
        throw new NotImplementedException($"BgtInstruction.{nameof(Execute)}");
    }
}

