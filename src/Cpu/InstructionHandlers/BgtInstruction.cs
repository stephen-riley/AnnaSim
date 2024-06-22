namespace AnnaSim.Instructions.Definitions;

public partial class BgtInstruction
{
    protected override uint ExecuteImpl(Instruction instruction) => BeqInstruction.ExecuteBranchOp(Cpu, instruction);
}

