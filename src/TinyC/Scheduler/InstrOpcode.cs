namespace AnnaSim.TinyC.Scheduler.Instructions;

public enum InstrOpcode
{
    Unknown = 0,
    Add,
    Sub,
    And,
    Or,
    Not,
    Jalr,
    In,
    Out,
    Addi,
    Shf,
    Lw,
    Sw,
    Lli,
    Lui,
    Lwi,
    Beq,
    Bne,
    Bgt,
    Bge,
    Blt,
    Ble,
    Mov,
    Push,
    Pop,
    _Org,
    _Fill,
    _Cstr,
    _Def,
    _Halt,
}