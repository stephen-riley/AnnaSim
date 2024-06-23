namespace AnnaSim.Instructions.Definitions;

public partial class AndInstruction
{
    protected override uint ExecuteImpl(Instruction instruction) => AddInstruction.ExecuteMathOp(Cpu, instruction);
}

