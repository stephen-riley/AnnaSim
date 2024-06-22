namespace AnnaSim.Instructions.Definitions;

public partial class BneInstruction
{
    protected override uint ExecuteImpl(Instruction instruction) => BeqInstruction.ExecuteBranchOp(Cpu, instruction);
}

