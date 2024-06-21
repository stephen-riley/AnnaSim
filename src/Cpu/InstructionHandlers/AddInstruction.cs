using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class AddInstruction
{
    public override uint Execute(AnnaMachine cpu, params string[] operands)
    {
        throw new NotImplementedException($"AddInstruction.{nameof(Execute)}");
    }
}

