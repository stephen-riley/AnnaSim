namespace AnnaSim.Instructions.Definitions;

public partial class BleInstruction
{
    protected override uint ExecuteImpl(Instruction instruction) => BeqInstruction.ExecuteBranchOp(Cpu, instruction);
}

