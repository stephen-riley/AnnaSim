using AnnaSim.Exceptions;
using AnnaSim.Extensions;
using AnnaSim.Test;
using static Opcode;

public class Instruction
{
    private readonly Word bits;

    public Instruction(ushort word) => bits = word;

    // RType MathOp constructor
    public Instruction(Opcode opcode, ushort rd, ushort rs1, ushort rs2, MathOp func)
    {
        if (opcode is not _Math or Jalr or In or Out)
        {
            throw new InvalidOpcodeException(opcode, "in Instruction RType math constructor");
        }

        bits = (uint)opcode << 12;
        bits |= (rd & 0b111u) << 9;
        bits |= (rs1 & 0b111u) << 6;
        bits |= (rs2 & 0b111u) << 3;
        bits |= (uint)func;
    }

    // RType/Imm6 constructor
    public Instruction(Opcode opcode, ushort rd, ushort rs1, short rs2orImm6)
    {
        if (opcode.IsImm8Type() || opcode == _Math)
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

    public Opcode Opcode => (Opcode)(bits >> 12);
    public uint Rd => (uint)((bits >> 9) & 0b111);
    public uint Rs1 => !Imm8Type ? (uint)((bits >> 6) & 0b111) : throw new InvalidInstructionFieldAccessException(Opcode, nameof(Rs1), "Instruction is not Rtype or I6Type");
    public uint Rs2 => RType ? (uint)((bits >> 3) & 0b111) : throw new InvalidInstructionFieldAccessException(Opcode, nameof(Rs2), "Instruction is not RType");
    public int Imm6 => Imm6Type ? ((int)bits & 0b111111).SignExtend(6) : throw new InvalidInstructionFieldAccessException(Opcode, nameof(Imm6), "Instruction is not I6Type");
    public int Imm8 => Imm8Type ? ((int)bits & 0b11111111).SignExtend(8) : throw new InvalidInstructionFieldAccessException(Opcode, nameof(Imm8), "Instruction is not I8Type");
    public MathOp FuncCode => RType && Opcode == _Math ? (MathOp)((ushort)bits & 0b111) : throw new InvalidInstructionFieldAccessException(Opcode, nameof(FuncCode), "Instruction is not RType or not a math operation");

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
}