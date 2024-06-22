using AnnaSim.Cpu;
using AnnaSim.Cpu.Memory;

namespace AnnaSim.Instructions.Definitions;

public partial class AddInstruction
{
    protected override uint ExecuteImpl(Instruction instruction) => ExecuteMathOp(Cpu, instruction);

    static internal uint ExecuteMathOp(AnnaMachine cpu, Instruction i)
    {
        var rs1 = i.Rs1;
        var rs2 = i.Rs2;
        var rs1val = (SignedWord)cpu.Registers[rs1];
        var rs2val = (SignedWord)cpu.Registers[rs2];

        SignedWord rdval = i.FuncCode switch
        {
            MathOperation.Add => rs1val + rs2val,
            MathOperation.Sub => rs1val - rs2val,
            MathOperation.And => rs1val & rs2val,
            MathOperation.Or => rs1val | rs2val,
            MathOperation.Not => ~rs1val,
            _ => throw new InvalidOperationException()
        };

        cpu.Registers[i.Rd] = (ushort)rdval;
        return i.Idef.NormalizePc(cpu.Pc + 1);
    }
}

