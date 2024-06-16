namespace AnnaSim.Cpu.Instructions;

public enum Opcode
{
    // for the math ops that use the func code
    _Math = 0,

    // other instructions
    Jalr,
    In,
    Out,
    Addi,
    Shf,
    Lw,
    Sw,
    Lli,
    Lui,
    Beq,
    Bne,
    Bgt,
    Bge,
    Blt,
    Ble,

    // assembler directives
    _Halt,
    _Fill,
    _Ralias,
}