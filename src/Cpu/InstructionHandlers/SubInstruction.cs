namespace AnnaSim.Instructions.Definitions;

public partial class SubInstruction
{
    protected override uint ExecuteImpl(Instruction instruction) => AddInstruction.ExecuteMathOp(Cpu, instruction);
}

