using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class OrInstruction
{
    public override uint Execute(AnnaMachine cpu, params string[] operands)
    {
        throw new NotImplementedException($"OrInstruction.{nameof(Execute)}");
    }
}

