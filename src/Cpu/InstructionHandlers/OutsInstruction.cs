using System.Text;
using AnnaSim.Cpu;

namespace AnnaSim.Instructions.Definitions;

public partial class OutsInstruction
{
    protected override uint ExecuteImpl(Instruction instruction)
    {
        if (instruction.Rd == 0 && instruction.Idef.MathOp == MathOperation.Halt)
        {
            Cpu.Status = CpuStatus.Halted;
            return Pc;
        }

        var value = Registers[instruction.Rd];
        var sb = new StringBuilder();
        while (instruction.Idef.Cpu.Memory[value] != 0)
        {
            sb.Append((char)instruction.Idef.Cpu.Memory[value++]);
        }
        Cpu.OutputStringCallback(sb.ToString());
        return NormalizePc(Pc + 1);
    }
}

