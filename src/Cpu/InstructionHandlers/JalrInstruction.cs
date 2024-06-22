namespace AnnaSim.Instructions.Definitions;

public partial class JalrInstruction
{
    protected override uint ExecuteImpl(Instruction instruction)
    {
        Registers[instruction.Rs1] = NormalizePc(Pc + 1);
        return Registers[instruction.Rd];
    }
}

