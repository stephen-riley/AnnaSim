using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class OutInstruction
{
    public override uint Execute(AnnaMachine cpu, params string[] operands)
    {
        throw new NotImplementedException($"OutInstruction.{nameof(Execute)}");
    }
}

