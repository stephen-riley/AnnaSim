using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class OutcInstruction
{
    protected override uint ExecuteImpl(Instruction instruction)
    {
        if (instruction.Rd == 0 && instruction.Idef.MathOp == MathOperation.Halt)
        {
            Cpu.Status = CpuStatus.Halted;
            return Pc;
        }

        var value = (char)instruction.Idef.Cpu.Memory[Registers[instruction.Rd]];
        Cpu.OutputCharCallback(value);
        return NormalizePc(Pc + 1);
    }
}

