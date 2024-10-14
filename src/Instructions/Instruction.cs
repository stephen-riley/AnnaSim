
using AnnaSim.Cpu.Memory;
using AnnaSim.Exceptions;
using AnnaSim.Extensions;

using static AnnaSim.Instructions.InstructionType;

namespace AnnaSim.Instructions;

public class Instruction
{
    public Word Bits { get; internal set; }

    public InstructionDefinition Idef { get; internal set; }

    public Instruction(InstructionDefinition idef)
    {
        Idef = idef;
        Bits = 0xffff;
    }

    public Instruction(InstructionDefinition idef, ushort word) : this(idef) => Bits = word;

    // RType constructor
    public Instruction(InstructionDefinition idef, ushort rd, ushort rs1, ushort rs2) : this(idef)
    {
        if (idef.Type is not R)
        {
            throw new InvalidOpcodeException(idef, "in Instruction RType math constructor");
        }

        Bits = idef.Opcode << 12;
        Bits |= (rd & 0b111u) << 9;
        Bits |= (rs1 & 0b111u) << 6;
        Bits |= (rs2 & 0b111u) << 3;
        Bits |= (uint)idef.MathOp;
    }

    // Imm6 constructor
    public Instruction(InstructionDefinition idef, ushort rd, ushort rs1, short rs2orImm6) : this(idef)
    {
        Bits = idef.Opcode << 12;
        Bits |= (rd & 0b111u) << 9;
        Bits |= (rs1 & 0b111u) << 6;

        if (idef.Type == R)
        {
            Bits |= ((ushort)rs2orImm6 & 0b111u) << 3;
        }
        else
        {
            Bits |= (ushort)rs2orImm6 & 0b111111u;
        }
    }

    public static Instruction NewRType(InstructionDefinition idef, ushort rd, ushort rs1, ushort rs2) => new(idef, rd, rs1, rs2);

    public static Instruction NewImm6(InstructionDefinition idef, ushort rd, ushort rs1, short imm6) => new(idef, rd, rs1, imm6);

    // Imm8 constructor
    public Instruction(InstructionDefinition idef, ushort rd, short imm8) : this(idef)
    {
        if (!(idef.Type == InstructionType.Imm8))
        {
            throw new InvalidOpcodeException(idef, "in Instruction Imm8Type constructor");
        }

        Bits = idef.Opcode << 12;
        Bits |= (rd & 0b111u) << 9;
        Bits |= (ushort)imm8 & 0b11111111u;
    }

    public static Instruction NewImm8(InstructionDefinition idef, ushort rd, short imm8) => new(idef, rd, imm8);

    public uint Opcode => (uint)(Bits >> 12);
    public uint Rd => (uint)((Bits >> 9) & 0b111);
    public uint Rs1 => !Imm8Type ? (uint)((Bits >> 6) & 0b111) : throw new InvalidInstructionFieldAccessException(Idef, nameof(Rs1), "Instruction is not Rtype or I6Type");
    public uint Rs2 => RType ? (uint)((Bits >> 3) & 0b111) : throw new InvalidInstructionFieldAccessException(Idef, nameof(Rs2), "Instruction is not RType");
    public int Imm6 => Imm6Type ? ((int)Bits & 0b111111).SignExtend(6) : throw new InvalidInstructionFieldAccessException(Idef, nameof(Imm6), "Instruction is not I6Type");
    public int Imm8 => Imm8Type ? ((int)Bits & 0b11111111).SignExtend(8) : throw new InvalidInstructionFieldAccessException(Idef, nameof(Imm8), "Instruction is not I8Type");
    public MathOperation FuncCode => RType && Opcode == ISA.MathOpcode ? (MathOperation)((ushort)Bits & 0b111) : throw new InvalidInstructionFieldAccessException(Idef, nameof(FuncCode), "Instruction is not RType or not a math operation");

    public bool IsHalt => Bits == 0xF000;

    public InstructionType Type => Idef.Type;

    public bool RType => Type == InstructionType.R;
    public bool Imm6Type => Type == InstructionType.Imm6;
    public bool Imm8Type => Type == InstructionType.Imm8;

    public static implicit operator Word(Instruction i) => i.Bits;

    public override string ToString()
    {
        int bits = Bits;

        if (bits == 0)
        {
            return "0x0000";
        }
        else if (bits == 0x3000)
        {
            return "halt";
        }

        var opcode = bits >> 12;
        var func = bits & 0x07;
        var imm8x = 0;
        if (Type == InstructionType.Imm8)
        {
            imm8x = Imm8;
            if (Idef.ToStringUnsigned && Imm8 < 0)
            {
                imm8x = 256 - -Imm8;
            }
        }

        var fmt = Idef.FormatString;
        if (fmt.EndsWith('I') && Rs1 == 0)
        {
            fmt = fmt[0..^1];
        }

        var res = string.Join(' ', fmt.Select((char c) => c switch
        {
            'm' => Idef.Mnemonic,
            'd' => $"r{Rd}",
            '1' => $"r{Rs1}",
            'I' => $"r{Rs1}",
            '2' => $"r{Rs2}",
            '6' => Imm6.ToString(),
            '8' => imm8x.ToString(),
            _ => throw new InvalidOperationException($"unknown format character '{c}")
        }));

        return res;
    }

    public uint Execute() => Idef.Execute(this);
}