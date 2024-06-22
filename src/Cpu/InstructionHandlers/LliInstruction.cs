using AnnaSim.Extensions;

namespace AnnaSim.Instructions.Definitions;

public partial class LliInstruction
{
    protected override uint ExecuteImpl(Instruction instruction)
    {
        Registers[instruction.Rd] = (uint)instruction.Imm8.SignExtend(8);
        return NormalizePc(Pc + 1);
    }
}

