using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class InInstruction
{
    public override uint Execute(AnnaMachine cpu, params string[] operands)
    {
        throw new NotImplementedException($"InInstruction.{nameof(Execute)}");
    }
}

