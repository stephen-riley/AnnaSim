namespace AnnaSim.Instructions.Definitions;

public partial class BgeInstruction
{
    protected override uint ExecuteImpl(Instruction instruction) => BeqInstruction.ExecuteBranchOp(Cpu, instruction);
}

