namespace AnnaSim.Instructions.Definitions;

public partial class NotInstruction
{
    protected override uint ExecuteImpl(Instruction instruction) => AddInstruction.ExecuteMathOp(Cpu, instruction);
}

