namespace AnnaSim.Instructions.Definitions;

public partial class MulInstruction
{
    protected override uint ExecuteImpl(Instruction instruction) => AddInstruction.ExecuteMathOp(Cpu, instruction);
}

