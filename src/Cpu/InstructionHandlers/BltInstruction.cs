namespace AnnaSim.Instructions.Definitions;

public partial class BltInstruction
{
    protected override uint ExecuteImpl(Instruction instruction) => BeqInstruction.ExecuteBranchOp(Cpu, instruction);
}

