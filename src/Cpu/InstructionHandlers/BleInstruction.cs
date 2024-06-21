using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class BleInstruction
{
    public override uint Execute(AnnaMachine cpu, params string[] operands)
    {
        throw new NotImplementedException($"BleInstruction.{nameof(Execute)}");
    }
}

