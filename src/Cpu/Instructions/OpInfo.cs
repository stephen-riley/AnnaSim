namespace AnnaSim.Cpu.Instructions;
using static AnnaSim.Cpu.Instructions.Opcode;
using static AnnaSim.Cpu.Instructions.MathOp;
using static AnnaSim.Cpu.Instructions.InstructionType;


[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Really want lowercase 🤷🏻‍♂️")]
public readonly record struct OpInfo(Opcode opcode, MathOp mathOp, InstructionType type, int opCount)
{
    public static readonly Dictionary<string, OpInfo> OpcodeMap = new()
    {
        // standard opcodes
        ["jalr"] = new OpInfo(Jalr, _Unused, R, 2),
        ["in"] = new OpInfo(In, _Unused, R, 1),
        ["out"] = new OpInfo(Out, _Unused, R, 1),
        ["addi"] = new OpInfo(Addi, _Unused, Imm6, 3),
        ["shf"] = new OpInfo(Shf, _Unused, Imm6, 3),
        ["lw"] = new OpInfo(Lw, _Unused, Imm6, 3),
        ["sw"] = new OpInfo(Sw, _Unused, Imm6, 3),
        ["lli"] = new OpInfo(Lli, _Unused, Imm8, 2),
        ["lui"] = new OpInfo(Lui, _Unused, Imm8, 2),
        ["beq"] = new OpInfo(Beq, _Unused, Imm8, 2),
        ["bne"] = new OpInfo(Bne, _Unused, Imm8, 2),
        ["bgt"] = new OpInfo(Bgt, _Unused, Imm8, 2),
        ["bge"] = new OpInfo(Bge, _Unused, Imm8, 2),
        ["blt"] = new OpInfo(Blt, _Unused, Imm8, 2),
        ["ble"] = new OpInfo(Ble, _Unused, Imm8, 2),

        // opcode 0x00 (math operations)
        ["add"] = new OpInfo(_Math, Add, R, 3),
        ["sub"] = new OpInfo(_Math, Sub, R, 3),
        ["and"] = new OpInfo(_Math, And, R, 3),
        ["or"] = new OpInfo(_Math, Or, R, 3),
        ["not"] = new OpInfo(_Math, Not, R, 2),

        // assembler directives
        [".halt"] = new OpInfo(_Halt, _Unused, Directive, 0),
        [".fill"] = new OpInfo(_Fill, _Unused, Directive, -1),
        [".ralias"] = new OpInfo(_Ralias, _Unused, Directive, 2),
    };
};