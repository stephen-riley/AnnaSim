
using AnnaSim.Instructions;
using static AnnaSim.Instructions.Opcode;
using static AnnaSim.Instructions.MathOperation;

namespace AnnaSim.Extensions;

public static class OpcodeExtensions
{
    public static bool IsRType(this Opcode opcode) => opcode is _Math or Jalr or In or Out;
    public static bool IsImm6Type(this Opcode opcode) => opcode is Addi or Shf or Lw or Sw;
    public static bool IsImm8Type(this Opcode opcode) => opcode is Lli or Lui or Beq or Bne or Bgt or Bge or Blt or Ble;
    public static bool IsBranch(this Opcode opcode) => opcode is Beq or Bne or Bgt or Bge or Blt or Ble;

    public static InstructionType Type(this Opcode opcode) =>
        opcode.IsRType() ? InstructionType.R
            : opcode.IsImm6Type() ? InstructionType.Imm6
                : opcode.IsImm8Type() ? InstructionType.Imm8
                    : InstructionType.Invalid;

    public static int RequiredOperands(this Opcode opcode, MathOperation mathOp) =>
        (opcode, mathOp) switch
        {
            (_Math, Not) => 2,
            (_Math, _) => 3,
            (In or Out, _) => 1,
            (Jalr, _) => 2,
            _ => 3
        };
}