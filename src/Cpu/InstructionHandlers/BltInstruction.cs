using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class BltInstruction
{
    public override uint Execute(AnnaMachine cpu, params string[] operands)
    {
        throw new NotImplementedException($"BltInstruction.{nameof(Execute)}");
    }
}

