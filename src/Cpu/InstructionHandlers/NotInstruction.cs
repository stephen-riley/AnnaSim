using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class NotInstruction
{
    public override uint Execute(AnnaMachine cpu, params string[] operands)
    {
        throw new NotImplementedException($"NotInstruction.{nameof(Execute)}");
    }
}

