using AnnaSim.Cpu;
using AnnaSim.Cpu.Memory;

namespace AnnaSim.Instructions;

public abstract partial class InstructionDefinition
{
    public AnnaMachine Cpu { get; set; } = null!;

    public MemoryFile Memory => Cpu.Memory;
    public RegisterFile Registers => Cpu.Registers;

    public uint Pc
    {
        get => Cpu.Pc;
        set { Cpu.Pc = value; }
    }

    public static void SetCpu(AnnaMachine cpu)
    {
        foreach (var idef in ISA.Lookup.Values)
        {
            idef.Cpu = cpu;
        }
    }

    internal uint NormalizePc(int addr) => (uint)((addr < 0 ? addr + Memory.Length : addr) % Memory.Length);
    internal uint NormalizePc(uint addr) => (uint)(addr % Memory.Length);

    public uint Execute(AnnaMachine cpu, Instruction instr)
    {
        Cpu = cpu;
        return Execute(instr);
    }

    public uint Execute(Instruction instr)
    {
        if (Cpu is null)
        {
            throw new NullReferenceException($"{nameof(Cpu)} must be set before use");
        }

        return ExecuteImpl(instr);
    }

    protected abstract uint ExecuteImpl(Instruction i);
}