using System.Text.RegularExpressions;
using AnnaSim.Cpu.Instructions;
using AnnaSim.Cpu.Memory;

using static AnnaSim.Cpu.Instructions.InstructionType;
using static AnnaSim.Cpu.Instructions.Opcode;
using static AnnaSim.Cpu.Instructions.MathOp;
using AnnaSim.Exceptions;
using AnnaSim.Extensions;

namespace AnnaSim.Assember;

public partial class AnnaAssembler
{
    internal readonly Dictionary<string, uint> labels = [];

    internal readonly Dictionary<uint, string> resolutionToDo = [];

    internal readonly Dictionary<string, string> registerAliases = [];

    public MemoryFile MemoryImage { get; internal set; }

    internal uint Addr = 0u;

    public AnnaAssembler(int memorySize = 65536)
    {
        MemoryImage = new MemoryFile(memorySize);
    }

    public void Assemble(IEnumerable<string> lines)
    {
        foreach (var line in lines.Select(l => Regex.Replace(l, @"#.*", "")))
        {
            var pieces = WhiteSpaceRegex().Split(line).Where(s => s != "").ToArray();

            if (pieces.Length > 0)
            {
                AssembleLine(pieces);
            }
        }
    }

    internal void AssembleLine(string[] pieces)
    {
        int idx = 0;
        if (pieces[0].EndsWith(':'))
        {
            labels[pieces[0][0..^1]] = Addr;
            idx++;
        }

        (var opcode, var mathop, var instrtype, var numargs) = OpInfo.OpcodeMap[pieces[idx]];
        var opinfo = OpInfo.OpcodeMap[pieces[idx]];
        var fillOkay = numargs == -1 && pieces[idx..].Length > 0;
        var mismatch = pieces[idx..].Length - 1 != numargs;

        if (!fillOkay && mismatch)
        {
            throw new InvalidOpcodeException($"{numargs} operands required for {opcode}/{mathop}, got {string.Join(' ', pieces[(idx + 1)..])}");
        }

        _ = (instrtype, mathop) switch
        {
            (Directive, _) => HandleDirective(opinfo, pieces[(idx + 1)..]),
            (R, _Unused) => HandleStandardOpcode(opinfo, pieces[(idx + 1)..]),
            (R, not _Unused) => HandleMathOpcode(opinfo, pieces[(idx + 1)..]),
            (Imm6, _) => HandleStandardOpcode(opinfo, pieces[(idx + 1)..]),
            (Imm8, _) => HandleStandardOpcode(opinfo, pieces[(idx + 1)..]),
            _ => throw new InvalidOpcodeException($"Cannot parse line {string.Join(' ', pieces)}")
        };
    }

    internal bool HandleDirective(OpInfo opinfo, string[] operands)
    {
        if (opinfo.opcode == _Halt)
        {
            MemoryImage[Addr++] = Instruction.Out(0);
        }
        else if (opinfo.opcode == _Fill)
        {
            foreach (var operand in operands)
            {
                MemoryImage[Addr++] = (SignedWord)ParseOperand(operand);
            }
        }
        else if (opinfo.opcode == _Ralias && operands[0].IsStandardRegisterName() && operands[1].StartsWith('r'))
        {
            registerAliases[operands[1]] = operands[0];
        }
        else
        {
            throw new InvalidOpcodeException($"cannot parse directive {opinfo.opcode} {string.Join(' ', operands)}");
        }

        return true;
    }

    internal bool HandleMathOpcode(OpInfo opinfo, string[] operands)
    {
        MemoryImage[Addr++] = opinfo.mathOp != Not
            ? Instruction.NewMathRtype(opinfo.mathOp, (ushort)ParseOperand(operands[0]), (ushort)ParseOperand(operands[1]), (ushort)ParseOperand(operands[2]))
            : Instruction.NewMathRtype(opinfo.mathOp, (ushort)ParseOperand(operands[0]), (ushort)ParseOperand(operands[1]));

        return true;
    }

    internal bool HandleStandardOpcode(OpInfo opInfo, string[] operands)
    {
        MemoryImage[Addr++] = (opInfo.type, opInfo.opcode) switch
        {
            (R, Jalr) => Instruction.NewRType(opInfo.opcode, (ushort)ParseOperand(operands[0]), (ushort)ParseOperand(operands[1]), 0xff),
            (R, In or Out) => Instruction.NewRType(opInfo.opcode, (ushort)ParseOperand(operands[0]), 0xff, 0xff),
            (R, _) => Instruction.NewRType(opInfo.opcode, (ushort)ParseOperand(operands[0]), (ushort)ParseOperand(operands[1]), 0xff),
            (Imm6, _) => Instruction.NewImm6(opInfo.opcode, (ushort)ParseOperand(operands[0]), (ushort)ParseOperand(operands[1]), (short)ParseOperand(operands[2])),
            (Imm8, _) => Instruction.NewImm8(opInfo.opcode, (ushort)ParseOperand(operands[0]), (short)ParseOperand(operands[1])),
            _ => throw new InvalidOpcodeException($"Cannot parse line {opInfo.opcode} {string.Join(' ', operands)}")
        };

        return true;
    }

    internal int ParseOperand(string o)
    {
        if (o.StartsWith('&'))
        {
            resolutionToDo[Addr] = o[1..];
            return -1;
        }
        else
        {
            return ParseNonLabelOperand(o);
        }
    }

    internal int ParseNonLabelOperand(string o)
    {
        if (o.StartsWith('r'))
        {
            return ParseRegister(o);
        }
        else if (o.StartsWith("0b"))
        {
            return Convert.ToInt32(o[2..], 2);
        }
        else if (o.StartsWith("0x"))
        {
            return Convert.ToInt32(o[2..], 16);
        }
        else
        {
            return Convert.ToInt32(o);
        }
    }

    internal ushort ParseRegister(string r)
    {
        if (r.StartsWith('r'))
        {
            var ridx = Convert.ToUInt16(r[1..]);
            if (ridx is >= 0 and <= 7)
            {
                return ridx;
            }
        }
        else if (registerAliases.TryGetValue(r, out var register))
        {
            return ParseRegister(register);
        }

        throw new InvalidOperationException($"'{r}' is not a valid register designator");
    }

    internal static void ValidateOperandCount(Opcode opcode, MathOp mathop, int count)
    {
        var requiredCount = opcode.RequiredOperands(mathop);
        if (requiredCount != count)
        {
            throw new InvalidOpcodeException($"{count} operands required for {opcode}/{mathop}");
        }
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhiteSpaceRegex();
}