using System.Text.RegularExpressions;
using AnnaSim.Instructions;
using AnnaSim.Cpu.Memory;
using AnnaSim.Exceptions;
using AnnaSim.Extensions;

namespace AnnaSim.Assembler;

public partial class AnnaAssembler
{
    internal readonly Dictionary<string, uint> labels = [];

    internal readonly Dictionary<uint, string> resolutionToDo = [];

    internal readonly Dictionary<string, string> registerAliases = [];

    internal readonly Dictionary<uint, int> lineMap = [];

    public MemoryFile MemoryImage { get; internal set; }

    internal uint Addr = 0u;

    public AnnaAssembler(int memorySize = 65536)
    {
        MemoryImage = new MemoryFile(memorySize);
        InstructionDefinition.SetAssembler(this);

        Enumerable.Range(0, 8).ForEach(n => registerAliases[$"r{n}"] = $"r{n}");
    }

    public AnnaAssembler(string filename, int memorySize = 65536)
        : this(memorySize)
    {
        Assemble(filename);
    }

    public void Assemble(string filename)
    {
        string[] lines;
        if (filename == "-" && Console.IsInputRedirected)
        {
            var list = new List<string>();
            string? line;
            while ((line = Console.ReadLine()) != null)
            {
                list.Add(line);
            }
            lines = [.. list];
            Console.Error.WriteLine("got these lines:\n" + string.Join("\n", lines));
            Assemble(lines);
        }
        else if (filename != "")
        {
            lines = File.ReadAllLines(filename);
            Assemble(lines);
        }
        else
        {
            return;
        }
    }

    public void Assemble(IEnumerable<string> lines)
    {
        foreach (var (index, line) in lines.SelectWithIndex(1, l => Regex.Replace(l, @"#.*", "")))
        {
            lineMap[Addr] = index;
            AssembleLine(line);
        }

        ResolveLabels();
    }

    internal void AssembleLine(string line)
    {
        var rawPieces = new Regex(@"\s+").Split(line).Where(s => s != "").ToArray();
        if (rawPieces.Length == 0)
        {
            return;
        }

        var pieces = new List<string>();

        var i = 0;
        do
        {
            if (rawPieces[i].StartsWith('"'))
            {
                pieces.Add(string.Join(" ", rawPieces[i..]));
                break;
            }

            pieces.Add(rawPieces[i]);
            i++;
        } while (i < rawPieces.Length);

        AssembleLine([.. pieces]);
    }

    internal void AssembleLine(string[] pieces)
    {
        try
        {
            int idx = 0;
            string? label = null;
            if (pieces[0].EndsWith(':'))
            {
                label = pieces[0][0..^1];
                labels[label] = Addr;
                idx++;
            }

            if (idx == pieces.Length)
            {
                return;
            }

            if (ISA.Lookup.TryGetValue(pieces[idx], out var def))
            {
                def.Asm = this;
                def.Assemble(pieces[(idx + 1)..].Select(s => ParseOperand(s)).ToArray(), label);
            }
            else
            {
                throw new AssemblerParseException(string.Join(' ', pieces));
            }
        }
        catch (Exception e)
        {
            throw new AssemblerParseException(string.Join(' ', pieces), e);
        }
    }

    internal void ResolveLabels()
    {
        foreach ((var addr, var label) in resolutionToDo)
        {
            if (labels.TryGetValue(label, out var targetAddr))
            {
                var wordAtAddr = MemoryImage[addr];
                var def = ISA.GetIdef(wordAtAddr);

                if (def.IsBranch)
                {
                    var offset = (int)targetAddr - ((int)addr + 1);
                    if (offset > MemoryImage.Length / 2)
                    {
                        offset -= MemoryImage.Length;
                    }
                    if (offset is > 127 or < -128)
                    {
                        throw new InvalidOpcodeException(def, "target address is too far away {offset}");
                    }

                    MemoryImage[addr] = MemoryImage[addr].SetLower(offset);
                }
                else if (def.Mnemonic == "lli")
                {
                    wordAtAddr = wordAtAddr.SetLower(targetAddr);
                    MemoryImage[addr] = wordAtAddr;
                }
                else if (def.Mnemonic == "lui")
                {
                    MemoryImage[addr] = MemoryImage[addr].SetLower(targetAddr >> 8);
                }
                else
                {
                    throw new InvalidOpcodeException(def, "cannot use label as operand");
                }
            }
            else
            {
                throw new LabelNotFoundException(label);
            }
        }
    }

    public static Operand Register(string r) => new(r, OperandType.Register);

    public static Operand Label(string l) => new(l, OperandType.Label);

    public Operand ParseOperand(string s, OperandType type = OperandType.SignedInt)
    {
        if (s.StartsWith('&'))
        {
            return Label(s[1..]);
        }
        else if (s.StartsWith('r'))
        {
            return Register(s);
        }
        else if (s.StartsWith('"'))
        {
            return new Operand(s, OperandType.String);
        }

        var value = ParseNumeric(s);
        return type == OperandType.SignedInt ? new Operand(value) : new Operand((uint)value);
    }

    public static int ParseNumeric(string s)
    {
        int negative = s.StartsWith('-') ? 1 : 0;
        int radix = s[negative..].StartsWith("0x") ? 16 : s[negative..].StartsWith("0b") ? 2 : 10;
        int value = Convert.ToInt32(s[(negative + (radix != 10 ? 2 : 0))..], radix);
        value = negative == 1 ? -value : value;
        return value;
    }

    public static bool TryParseNumeric(string s, out int n)
    {
        try
        {
            n = ParseNumeric(s);
            return true;
        }
        catch
        {
            n = -1;
            return false;
        }
    }

    internal ushort Register(Operand o)
    {
        if (o.Type == OperandType.Register)
        {
            var r = registerAliases[o.Str];
            return Convert.ToUInt16(r[1..]);
        }
        else if (o.Type == OperandType.UnsignedInt)
        {
            return (ushort)o.UnsignedInt;
        }
        else
        {
            throw new InvalidCastException($"Operand is of type {o.Type}, requested Register");
        }
    }

    public PdbInfo GetPdb() => new()
    {
        Labels = labels,
        RegisterAliases = registerAliases,
        LineAddrMap = lineMap.Select(kvp => KeyValuePair.Create(kvp.Value, kvp.Key)).ToDictionary(),
    };
}