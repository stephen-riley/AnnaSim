namespace AnnaSim.Instructions;

public enum InstructionType
{
    Invalid = -1,
    R = 0,
    Imm6,
    Imm8,

    // used for assembler directives
    Directive
}