using System.Text.RegularExpressions;
using AnnaSim.Instructions;
using AnnaSim.Cpu.Memory;

using static AnnaSim.Instructions.InstructionType;
using static AnnaSim.Instructions.Opcode;
using static AnnaSim.Instructions.MathOperation;
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

    public AnnaAssembler(string filename, int memorySize = 65536)
        : this(memorySize)
    {
        Assemble(filename);
    }

    public void Assemble(string filename)
    {
        Assemble(File.ReadAllLines(filename));
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

        ResolveLabels();
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
        var fillOkay = numargs == -1 && pieces.Length > 1;
        var mismatch = pieces[idx..].Length - 1 != numargs;

        if (!fillOkay && mismatch)
        {
            throw new InvalidOpcodeException($"{numargs} operands required for {opcode}/{mathop}, got {string.Join(' ', pieces[(idx + 1)..])}");
        }

        _ = (instrtype, mathop) switch
        {
            (Directive, _) => HandleDirective(opinfo, pieces[(idx + 1)..]),
            (R, NA) => HandleStandardOpcode(opinfo, pieces[(idx + 1)..]),
            (R, not NA) => HandleMathOpcode(opinfo, pieces[(idx + 1)..]),
            (Imm6, _) => HandleStandardOpcode(opinfo, pieces[(idx + 1)..]),
            (Imm8, _) => HandleStandardOpcode(opinfo, pieces[(idx + 1)..]),
            _ => throw new InvalidOpcodeException($"Cannot parse line {string.Join(' ', pieces)}")
        };
    }

    internal void ResolveLabels()
    {
        foreach ((var addr, var label) in resolutionToDo)
        {
            if (labels.TryGetValue(label, out var targetAddr))
            {
                var wordAtAddr = MemoryImage[addr];
                var instruction = wordAtAddr.ToInstruction();

                if (instruction.Opcode.IsBranch())
                {
                    var offset = (int)targetAddr - ((int)addr + 1);
                    if (offset > MemoryImage.Length / 2)
                    {
                        offset -= MemoryImage.Length;
                    }
                    if (offset is > 127 or < -128)
                    {
                        throw new InvalidOpcodeException(instruction.Opcode, "target address is too far away {offset}");
                    }

                    MemoryImage[addr] = MemoryImage[addr].SetLower(offset);
                }
                else if (instruction.Opcode == Lli)
                {
                    wordAtAddr = wordAtAddr.SetLower(targetAddr);
                    MemoryImage[addr] = wordAtAddr;
                }
                else if (instruction.Opcode == Lui)
                {
                    MemoryImage[addr] = MemoryImage[addr].SetLower(targetAddr >> 8);
                }
                else
                {
                    throw new InvalidOpcodeException(instruction.Opcode, "cannot use label as operand");
                }
            }
            else
            {
                throw new LabelNotFoundException(label);
            }
        }
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
                MemoryImage[Addr] = (SignedWord)ParseOperand(operand);
                Addr++; // do this after ParseOperand() in case the operand is a label
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
        // This is such a hack to handle 16-bit operands for Lui 🤦🏻‍♂️
        // TODO: fix this eggregiousness (go to a class per instruction?)
        var operand1 = 0;
        if (operands.Length > 1)
        {
            operand1 = ParseOperand(operands[1]);
            if (opInfo.opcode == Lui)
            {
                operand1 >>= 8;
            }
        }

        MemoryImage[Addr] = (opInfo.type, opInfo.opcode) switch
        {
            (R, Jalr) => Instruction.NewRType(opInfo.opcode, (ushort)ParseOperand(operands[0]), (ushort)operand1, 0x0),
            (R, In or Out) => Instruction.NewRType(opInfo.opcode, (ushort)ParseOperand(operands[0]), 0x0, 0x0),
            (R, _) => Instruction.NewRType(opInfo.opcode, (ushort)ParseOperand(operands[0]), (ushort)operand1, 0x0),
            (Imm6, _) => Instruction.NewImm6(opInfo.opcode, (ushort)ParseOperand(operands[0]), (ushort)operand1, (short)ParseOperand(operands[2])),
            (Imm8, Lui) => Instruction.NewImm8(opInfo.opcode, (ushort)ParseOperand(operands[0]), (short)operand1),
            (Imm8, _) => Instruction.NewImm8(opInfo.opcode, (ushort)ParseOperand(operands[0]), (short)operand1),
            _ => throw new InvalidOpcodeException($"Cannot parse line {opInfo.opcode} {string.Join(' ', operands)}")
        };

        Addr++; // do this after ParseOperand() in case the operand is a label

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

    internal static void ValidateOperandCount(Opcode opcode, MathOperation mathop, int count)
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