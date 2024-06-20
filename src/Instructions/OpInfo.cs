namespace AnnaSim.Instructions;

using static AnnaSim.Instructions.Opcode;
using static AnnaSim.Instructions.MathOperation;
using static AnnaSim.Instructions.InstructionType;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Really want lowercase 🤷🏻‍♂️")]
public readonly record struct OpInfo(Opcode opcode, MathOperation mathOp, InstructionType type, int opCount)
{
    public static readonly Dictionary<string, OpInfo> OpcodeMap = new()
    {
        // standard opcodes
        ["jalr"] = new OpInfo(Jalr, NA, R, 2),
        ["in"] = new OpInfo(In, NA, R, 1),
        ["out"] = new OpInfo(Out, NA, R, 1),
        ["addi"] = new OpInfo(Addi, NA, Imm6, 3),
        ["shf"] = new OpInfo(Shf, NA, Imm6, 3),
        ["lw"] = new OpInfo(Lw, NA, Imm6, 3),
        ["sw"] = new OpInfo(Sw, NA, Imm6, 3),
        ["lli"] = new OpInfo(Lli, NA, Imm8, 2),
        ["lui"] = new OpInfo(Lui, NA, Imm8, 2),
        ["beq"] = new OpInfo(Beq, NA, Imm8, 2),
        ["bne"] = new OpInfo(Bne, NA, Imm8, 2),
        ["bgt"] = new OpInfo(Bgt, NA, Imm8, 2),
        ["bge"] = new OpInfo(Bge, NA, Imm8, 2),
        ["blt"] = new OpInfo(Blt, NA, Imm8, 2),
        ["ble"] = new OpInfo(Ble, NA, Imm8, 2),

        // opcode 0x00 (math operations)
        ["add"] = new OpInfo(_Math, Add, R, 3),
        ["sub"] = new OpInfo(_Math, Sub, R, 3),
        ["and"] = new OpInfo(_Math, And, R, 3),
        ["or"] = new OpInfo(_Math, Or, R, 3),
        ["not"] = new OpInfo(_Math, Not, R, 2),

        // assembler directives
        [".halt"] = new OpInfo(_Halt, NA, Directive, 0),
        [".fill"] = new OpInfo(_Fill, NA, Directive, -1),
        [".ralias"] = new OpInfo(_Ralias, NA, Directive, 2),
    };
};