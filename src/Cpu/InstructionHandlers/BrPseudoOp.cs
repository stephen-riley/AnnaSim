using AnnaSim.Cpu;
using AnnaSim.Cpu.Memory;

namespace AnnaSim.Instructions.Definitions;

public partial class BrPseudoOp
{
    protected override uint ExecuteImpl(Instruction instruction) => ExecuteBranchOp(Cpu, instruction);

    static internal uint ExecuteBranchOp(AnnaMachine cpu, Instruction instruction)
        => BeqInstruction.ExecuteBranchOp(cpu, instruction);
}

