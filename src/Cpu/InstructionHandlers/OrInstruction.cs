namespace AnnaSim.Instructions.Definitions;

public partial class OrInstruction
{
    protected override uint ExecuteImpl(Instruction instruction) => AddInstruction.ExecuteMathOp(Cpu, instruction);

}

