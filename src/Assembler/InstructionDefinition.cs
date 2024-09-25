using AnnaSim.AsmParsing;
using AnnaSim.Assembler;
using AnnaSim.Cpu.Memory;
using AnnaSim.Extensions;

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


    public void Assemble(CstInstruction ci)
    {
        if (Asm is null)
        {
            throw new NullReferenceException($"{nameof(Asm)} must be set before use");
        }

        ValidateOperands(ci.Operands);
        AssembleImpl(ci);
    }

    public void Assemble(AnnaAssembler asm, CstInstruction ci)
    {
        Asm = asm;
        Assemble(ci);
    }

    protected abstract void AssembleImpl(Operand[] operands, string? label);

    protected abstract void AssembleImpl(CstInstruction ci);

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

    public Instruction ToInstruction(Operand[] operands)
    {
        // operands.Where(o => o.Type == OperandType.Label).ForEach(o => Asm.resolutionToDo[Addr] = o.Str);
        return ToInstructionImpl(operands);
    }

    public abstract Instruction ToInstructionImpl(Operand[] operands);
}