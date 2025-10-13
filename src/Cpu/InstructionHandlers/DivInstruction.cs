namespace AnnaSim.Instructions.Definitions;

public partial class DivInstruction
{
    protected override uint ExecuteImpl(Instruction instruction) => AddInstruction.ExecuteMathOp(Cpu, instruction);
}

