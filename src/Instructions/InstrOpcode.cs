namespace AnnaSim.Instructions;

public enum InstrOpcode
{
    Unknown = 0,

    // Instructions
    Add,
    Sub,
    And,
    Or,
    Not,
    Mul,
    Div,
    Mod,
    Jalr,
    In,
    Out,
    Outn,
    Outs,
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

    // Pseudo-ops
    Br,
    Halt,
    Jmp,
    Lwi,
    Mov,
    Pop,
    Push,

    // Directives
    _Cstr,
    _Def,
    _Fill,
    _Halt,
    _Org,
    _Ralias,
}