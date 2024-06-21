using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class LuiInstruction
{
    public override uint Execute(AnnaMachine cpu, params string[] operands)
    {
        throw new NotImplementedException($"LuiInstruction.{nameof(Execute)}");
    }
}

