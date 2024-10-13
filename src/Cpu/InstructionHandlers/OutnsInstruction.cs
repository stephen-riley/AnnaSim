using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class OutnInstruction
{
    protected override uint ExecuteImpl(Instruction instruction)
    {
        if (instruction.Rd == 0 && instruction.Idef.MathOp == MathOperation.Halt)
        {
            Cpu.Status = CpuStatus.Halted;
            return Pc;
        }

        var value = (int)Registers[instruction.Rd];
        value = value > 32767 ? value - 65536 : value;
        Cpu.OutputStringCallback(value.ToString());
        return NormalizePc(Pc + 1);
    }
}

