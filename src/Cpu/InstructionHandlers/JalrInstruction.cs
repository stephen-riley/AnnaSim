using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class JalrInstruction
{
    public override uint Execute(AnnaMachine cpu, params string[] operands)
    {
        throw new NotImplementedException($"JalrInstruction.{nameof(Execute)}");
    }
}

