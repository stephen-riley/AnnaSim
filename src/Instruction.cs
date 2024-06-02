using AnnaSim.Exceptions;
using static Opcode;

public class Instruction
{
    private readonly Word word;

    public Instruction(ushort word) => this.word = word;

    // RType MathOp constructor
    public Instruction(Opcode opcode, ushort rd, ushort rs1, ushort rs2, MathOp func)
    {
        if (opcode is not _Math or Jalr or In or Out)
        {
            throw new InvalidOpcodeException(opcode, "in Instruction RType math constructor");
        }

        word = (uint)opcode << 12;
        word |= (rd & 0b111u) << 9;
        word |= (rs1 & 0b111u) << 6;
        word |= (rs2 & 0b111u) << 3;
        word |= (uint)func;
    }

    // RType/Imm6 constructor
    public Instruction(Opcode opcode, ushort rd, ushort rs1, ushort rs2OrImm6)
    {
        if (opcode.IsImm8Type() || opcode == _Math)
        {
            throw new InvalidOpcodeException(opcode, "in Instruction RsType/Imm6Type constructor");
        }

        word = (uint)opcode << 12;
        word |= (rd & 0b111u) << 9;
        word |= (rs1 & 0b111u) << 6;

        if (opcode.IsRType())
        {
            word |= (rs2OrImm6 & 0b111u) << 3;
        }
        else
        {
            word |= rs2OrImm6 & 0b111111u;
        }
    }

    // Imm8 constructor
    public Instruction(Opcode opcode, ushort rd, ushort imm8)
    {
        if (!opcode.IsImm8Type())
        {
            throw new InvalidOpcodeException(opcode, "in Instruction Imm8Type constructor");
        }

        word = (uint)opcode << 12;
        word |= (rd & 0b111u) << 9;
        word |= imm8 & 0b11111111u;
    }

    public Opcode Opcode => (Opcode)(word >> 12);
    public uint Rd => (uint)((word >> 9) & 0b111);
    public uint Rs1 => !Imm8Type ? (uint)((word >> 6) & 0b111) : throw new InvalidInstructionFieldAccessException(Opcode, nameof(Rs1), "Instruction is not Rtype or I6Type");
    public uint Rs2 => RType ? (uint)((word >> 3) & 0b111) : throw new InvalidInstructionFieldAccessException(Opcode, nameof(Rs2), "Instruction is not RType");
    public uint Imm6 => Imm6Type ? (uint)(word & 0b111111) : throw new InvalidInstructionFieldAccessException(Opcode, nameof(Imm6), "Instruction is not I6Type");
    public uint Imm8 => Imm8Type ? (uint)(word & 0b11111111) : throw new InvalidInstructionFieldAccessException(Opcode, nameof(Imm8), "Instruction is not I8Type");
    public MathOp FuncCode => RType && Opcode == _Math ? (MathOp)((ushort)word & 0b111) : throw new InvalidInstructionFieldAccessException(Opcode, nameof(FuncCode), "Instruction is not RType or not a math operation");

    public bool IsHalt => word == 0xF000;

    public InstructionType Type =>
        Opcode.IsRType() ? InstructionType.R
            : Opcode.IsImm6Type() ? InstructionType.Imm6
                : Opcode.IsImm8Type() ? InstructionType.Imm8
                    : throw new InvalidOpcodeException(Opcode, nameof(Type));

    public bool RType => Type == InstructionType.R;
    public bool Imm6Type => Type == InstructionType.Imm6;
    public bool Imm8Type => Type == InstructionType.Imm8;
}