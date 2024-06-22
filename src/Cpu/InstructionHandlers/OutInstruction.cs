using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class OutInstruction
{
    protected override uint ExecuteImpl(Instruction instruction)
    {
        if (instruction.Rd == 0)
        {
            Cpu.Status = CpuStatus.Halted;
            return Pc;
        }

        var value = Registers[instruction.Rd];
        Cpu.OutputCallback(value);
        return NormalizePc(Pc + 1);
    }
}

