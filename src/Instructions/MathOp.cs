using System.Diagnostics.CodeAnalysis;

namespace AnnaSim.Instructions;

[SuppressMessage("Design", "CA1069:Enums values should not be duplicated", Justification = "need NA and Add to have same value")]
public enum MathOperation
{
    NA = 0,
    Add = 0,
    Sub,
    And,
    Or,
    Not,
    Mul,
    Div,
    Mod,

    Halt = 0,
    OutString = 1,
    OutNumString = 2,
}