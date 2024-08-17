using AnnaSim.Assembler;
using AnnaSim.Cpu.Memory;

namespace AnnaSim.Instructions;

public abstract partial class InstructionDefinition
{
    public AnnaAssembler Asm { get; set; } = null!;

    internal MemoryFile MemoryImage => Asm.MemoryImage;

    internal uint Addr
    {
        get { return Asm.Addr; }
        set { Asm.Addr = value; }
    }

    public static void SetAssembler(AnnaAssembler asm)
    {
        foreach (var idef in ISA.Lookup.Values)
        {
            idef.Asm = asm;
        }
    }

    public void Assemble(AnnaAssembler asm, Operand[] operands, string? label = null)
    {
        Asm = asm;
        Assemble(operands, label);
    }

    public void Assemble(Operand[] operands, string? label = null)
    {
        if (Asm is null)
        {
            throw new NullReferenceException($"{nameof(Asm)} must be set before use");
        }

        ValidateOperands(operands);
        AssembleImpl(operands, label);
    }

    protected abstract void AssembleImpl(Operand[] operands, string? label);

    public Instruction ToInstruction(uint? rd = null, uint? rs1 = null, uint? rs2 = null, int? imm6 = null, int? imm8 = null)
    {
        var operands = new List<Operand>();
        if (rd is not null) operands.Add(rd);
        if (rs1 is not null) operands.Add(rs1);
        if (rs2 is not null) operands.Add(rs2);
        if (imm6 is not null) operands.Add(imm6);
        if (imm8 is not null) operands.Add(imm8);

        return ToInstruction([.. operands]);
    }

    public abstract Instruction ToInstruction(Operand[] operands);
}