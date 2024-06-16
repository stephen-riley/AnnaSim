
using AnnaSim.Cpu.Memory;
using AnnaSim.Exceptions;
using AnnaSim.Extensions;

using static AnnaSim.Cpu.Instructions.InstructionType;

namespace AnnaSim.Cpu.Instructions;

public class Instruction
{
    private readonly Word bits;

    public Instruction(ushort word) => bits = word;

    // RType MathOp constructor
    public Instruction(Opcode opcode, ushort rd, ushort rs1, ushort rs2, MathOp func)
    {
        if (opcode is not Opcode._Math or Opcode.Jalr or Opcode.In or Opcode.Out)
        {
            throw new InvalidOpcodeException(opcode, "in Instruction RType math constructor");
        }

        bits = (uint)opcode << 12;
        bits |= (rd & 0b111u) << 9;
        bits |= (rs1 & 0b111u) << 6;
        bits |= (rs2 & 0b111u) << 3;
        bits |= (uint)func;
    }

    public static Instruction NewMathRtype(MathOp func, ushort rd, ushort rs1, ushort rs2) => new(Opcode._Math, rd, rs1, rs2, func);

    public static Instruction NewMathRtype(MathOp _, ushort rd, ushort rs1) => new(Opcode._Math, rd, rs1, 0xffff, MathOp.Not);

    // RType/Imm6 constructor
    public Instruction(Opcode opcode, ushort rd, ushort rs1, short rs2orImm6)
    {
        if (opcode.IsImm8Type() || opcode == Opcode._Math)
        {
            throw new InvalidOpcodeException(opcode, "in Instruction RsType/Imm6Type constructor");
        }

        bits = (uint)opcode << 12;
        bits |= (rd & 0b111u) << 9;
        bits |= (rs1 & 0b111u) << 6;

        if (opcode.IsRType())
        {
            bits |= ((ushort)rs2orImm6 & 0b111u) << 3;
        }
        else
        {
            bits |= (ushort)rs2orImm6 & 0b111111u;
        }
    }

    public static Instruction NewRType(Opcode opcode, ushort rd, ushort rs1, ushort rs2) => new(opcode, rd, rs1, (short)rs2);

    public static Instruction NewImm6(Opcode opcode, ushort rd, ushort rs1, short imm6) => new(opcode, rd, rs1, imm6);

    // Imm8 constructor
    public Instruction(Opcode opcode, ushort rd, short imm8)
    {
        if (!opcode.IsImm8Type())
        {
            throw new InvalidOpcodeException(opcode, "in Instruction Imm8Type constructor");
        }

        bits = (uint)opcode << 12;
        bits |= (rd & 0b111u) << 9;
        bits |= (ushort)imm8 & 0b11111111u;
    }

    public static Instruction NewImm8(Opcode opcode, ushort rd, short imm8) => new Instruction(opcode, rd, imm8);

    public Opcode Opcode => (Opcode)(bits >> 12);
    public uint Rd => (uint)((bits >> 9) & 0b111);
    public uint Rs1 => !Imm8Type ? (uint)((bits >> 6) & 0b111) : throw new InvalidInstructionFieldAccessException(Opcode, nameof(Rs1), "Instruction is not Rtype or I6Type");
    public uint Rs2 => RType ? (uint)((bits >> 3) & 0b111) : throw new InvalidInstructionFieldAccessException(Opcode, nameof(Rs2), "Instruction is not RType");
    public int Imm6 => Imm6Type ? ((int)bits & 0b111111).SignExtend(6) : throw new InvalidInstructionFieldAccessException(Opcode, nameof(Imm6), "Instruction is not I6Type");
    public int Imm8 => Imm8Type ? ((int)bits & 0b11111111).SignExtend(8) : throw new InvalidInstructionFieldAccessException(Opcode, nameof(Imm8), "Instruction is not I8Type");
    public MathOp FuncCode => RType && Opcode == Opcode._Math ? (MathOp)((ushort)bits & 0b111) : throw new InvalidInstructionFieldAccessException(Opcode, nameof(FuncCode), "Instruction is not RType or not a math operation");

    public bool IsHalt => bits == 0xF000;

    public InstructionType Type =>
        Opcode.IsRType() ? InstructionType.R
            : Opcode.IsImm6Type() ? InstructionType.Imm6
                : Opcode.IsImm8Type() ? InstructionType.Imm8
                    : throw new InvalidOpcodeException(Opcode, nameof(Type));

    public bool RType => Type == InstructionType.R;
    public bool Imm6Type => Type == InstructionType.Imm6;
    public bool Imm8Type => Type == InstructionType.Imm8;

    public static implicit operator Word(Instruction i) => i.bits;

    public override string ToString()
    {
        int bits = this.bits;

        if (bits == 0)
        {
            return "0x0000";
        }
        else if (bits == 0x3000)
        {
            return ".halt";
        }

        var opcode = bits >> 12;
        var func = bits & 0x07;
        var rd = bits >> 9 & 0x07;
        var rs1 = bits >> 6 & 0x07;
        var rs2 = bits >> 3 & 0x07;
        var imm6 = (bits & 0x3f).SignExtend(6);
        var imm8 = (bits & 0xff).SignExtend(8);
        var imm8u = bits & 0xff;

        var opname = opcode == (uint)Opcode._Math
            ? ((MathOp)func).ToString().ToLower()
            : ((Opcode)opcode).ToString().ToLower();

        return (Type, (Opcode)opcode, (MathOp)func) switch
        {
            (_, Opcode._Math, MathOp.Not) => $"{opname} r{rd} r{rs1}",
            (_, Opcode._Math, _) => $"{opname} r{rd} r{rs1} r{rs2}",
            (R, Opcode.Jalr, _) => rs1 == 0 ? $"{opname} r{rd}" : $"{opname} r{rd} r{rs1}",
            (R, Opcode.In or Opcode.Out, _) => $"{opname} r{rd}",
            (InstructionType.Imm6, _, _) => $"{opname} r{rd} r{rs1} {imm6}",
            (InstructionType.Imm8, Opcode.Lui or Opcode.Lli, _) => $"{opname} r{rd} {imm8u}",
            (InstructionType.Imm8, _, _) => $"{opname} r{rd} {imm8}",
            _ => "unknown"
        };
    }

    public static Instruction Add(ushort rd, ushort rs1, ushort rs2) => new(Opcode._Math, rd, rs1, rs2, MathOp.Add);

    public static Instruction Sub(ushort rd, ushort rs1, ushort rs2) => new(Opcode._Math, rd, rs1, rs2, MathOp.Sub);

    public static Instruction And(ushort rd, ushort rs1, ushort rs2) => new(Opcode._Math, rd, rs1, rs2, MathOp.And);

    public static Instruction Or(ushort rd, ushort rs1, ushort rs2) => new(Opcode._Math, rd, rs1, rs2, MathOp.Or);

    public static Instruction Not(ushort rd, ushort rs1, ushort rs2) => new(Opcode._Math, rd, rs1, rs2, MathOp.Not);

    public static Instruction Jalr(ushort rd, ushort rs1) => new(Opcode.Jalr, rd, rs1, 0);

    public static Instruction Jalr(ushort rd) => new(Opcode.Jalr, rd, 0, 0);

    public static Instruction In(ushort rd) => new(Opcode.In, rd, 0, 0);

    public static Instruction Out(ushort rd) => new(Opcode.Out, rd, 0, 0);

    public static Instruction Addi(ushort rd, ushort rs1, SignedWord imm6) => new(Opcode.Addi, rd, rs1, imm6);

    public static Instruction Shf(ushort rd, ushort rs1, SignedWord imm6) => new(Opcode.Shf, rd, rs1, imm6);

    public static Instruction Lw(ushort rd, ushort rs1, SignedWord imm6) => new(Opcode.Lw, rd, rs1, imm6);

    public static Instruction Sw(ushort rd, ushort rs1, SignedWord imm6) => new(Opcode.Sw, rd, rs1, imm6);

    public static Instruction Lli(ushort rd, SignedWord imm8) => new(Opcode.Lli, rd, imm8);

    public static Instruction Lui(ushort rd, SignedWord imm8) => new(Opcode.Lui, rd, imm8);

    public static Instruction Beq(ushort rd, SignedWord imm8) => new(Opcode.Beq, rd, imm8);

    public static Instruction Bne(ushort rd, SignedWord imm8) => new(Opcode.Bne, rd, imm8);

    public static Instruction Bgt(ushort rd, SignedWord imm8) => new(Opcode.Bgt, rd, imm8);

    public static Instruction Bge(ushort rd, SignedWord imm8) => new(Opcode.Bge, rd, imm8);

    public static Instruction Blt(ushort rd, SignedWord imm8) => new(Opcode.Blt, rd, imm8);

    public static Instruction Ble(ushort rd, SignedWord imm8) => new(Opcode.Ble, rd, imm8);
}