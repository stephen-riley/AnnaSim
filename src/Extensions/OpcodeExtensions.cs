namespace AnnaSim.Extensions;

using static Opcode;

public static class OpcodeExtensions
{
    public static bool IsRType(this Opcode opcode) => opcode is _Math or Jalr or In or Out;
    public static bool IsImm6Type(this Opcode opcode) => opcode is Addi or Shf or Lw or Sw;
    public static bool IsImm8Type(this Opcode opcode) => opcode is Lli or Lui or Beq or Bne or Bgt or Bge or Blt or Ble;
    public static bool IsBranch(this Opcode opcode) => opcode is Beq or Bne or Bgt or Bge or Blt or Ble;
}