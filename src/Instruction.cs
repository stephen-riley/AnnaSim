public class Instruction(ushort word)
{
    private ushort word = word;

    public Opcode Opcode => (Opcode)(word >> 12);
    public uint Rd => (uint)((word >> 8) & 0b111);
    public uint Rs1 => (uint)((word >> 5) & 0b111);
    public uint Rs2 => (uint)((word >> 3) & 0b111);
    public uint Imm6 => (uint)(word & 0b111111);
    public uint Imm8 => (uint)(word & 0b11111111);
    public MathOp FuncCode => (MathOp)(word & 0b111);

    public bool IsHalt => word == 0xF000;

    public InstructionType Type => (word >> 11) switch
    {
        <= 3 => InstructionType.R,
        > 3 and <= 7 => InstructionType.Imm6,
        > 7 and <= 15 => InstructionType.Imm8,
        _ => throw new InvalidOperationException()
    };
}