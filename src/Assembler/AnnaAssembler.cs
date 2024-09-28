using AnnaSim.Instructions;
using AnnaSim.Cpu.Memory;
using AnnaSim.Exceptions;
using AnnaSim.Extensions;
using AnnaSim.AsmParsing;

namespace AnnaSim.Assembler;

public partial class AnnaAssembler
{
    internal readonly Dictionary<string, uint> labels = [];

    // key tuple is (CstInstruction index, AssembledWords[] offset)
    internal readonly Dictionary<(uint, int), string> resolutionToDo = [];

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
            // Console.Error.WriteLine("got these lines:\n" + string.Join("\n", lines));
        }
        else if (filename != "")
        {
            lines = File.ReadAllLines(filename);
        }
        else
        {
            return;
        }

        var cis = Assemble(lines);
        var fw = new StreamWriter("/tmp/out.dasm") { AutoFlush = true };
        foreach (var i in cis)
        {
            i.Render(fw, true);
        }
    }

    public IEnumerable<CstInstruction> Assemble(IEnumerable<string> lines)
    {
        // First build a list of CstInstructions.
        var instructions = CstParser.ParseLines(lines);

        // Now assemble them.  The resulting bits are stored in the
        //  CstInstructions themselves.
        foreach (var ci in instructions)
        {
            try
            {
                if (ISA.Lookup.TryGetValue(ci.Mnemonic, out var def))
                {
                    def.Asm = this;
                    ci.Def = def;
                    foreach (var l in ci.Labels)
                    {
                        labels[l] = Addr;
                    }

                    def.Assemble(ci);
                }
                else
                {
                    throw new AssemblerParseException("unknown reason");
                }
            }
            catch (Exception e)
            {
                throw new AssemblerParseException("TODO", e);
            }
        }

        // Now resolve any labels.
        ResolveLabels(instructions);

        // Finally, put all the CstInstructions' bits into the MemoryImage.
        foreach (var ci in instructions)
        {
            ci.AssembledWords.Each((w, offset) =>
            {
                MemoryImage[ci.BaseAddress + (uint)offset] = w;
            });
        }

        return instructions;
    }

    internal void ResolveLabels(IEnumerable<CstInstruction> instructions)
    {
        var ciTbd = instructions.ThatOccupyMemory();
        var ciMap = ciTbd.ToDictionary(i => i.BaseAddress, i => i);

        foreach (var ci in ciTbd)
        {
            var labelOperand = ci.Operands.Where(o => o.Type == OperandType.Label);
            if (labelOperand.Any())
            {
                // Most of the pseudo-ops expand to two instructions, so we need to
                //  apply the label to both.  However, the .fill directive can have
                //  a mix of labels and non-labels.
                if (ci.Opcode == InstrOpcode._Fill)
                {
                    ci.AssembledWords.Each((w, offset) =>
                    {
                        if (ci.Operands[offset].Type == OperandType.Label)
                        {
                            resolutionToDo[(ci.BaseAddress, offset)] = ci.Operands[offset].Str;
                        }
                    });
                }
                else
                {
                    var label = labelOperand.First().Str;
                    ci.AssembledWords.Each((w, offset) => resolutionToDo[(ci.BaseAddress, offset)] = label);
                }
            }
        }

        foreach (((var baseAddr, var offset), var label) in resolutionToDo)
        {
            if (labels.TryGetValue(label[1..], out var targetAddr))
            {
                var ci = ciMap[baseAddr];

                if (ci.Opcode == InstrOpcode._Fill)
                {
                    ci.AssembledWords[offset] = targetAddr;
                }
                else
                {
                    // Now we have to get the idef object from the wordAtAddr
                    //  because the CI we have here might be a pseudo-op
                    //  or directive with >1 words.
                    var wordAtAddr = ci.AssembledWords[offset];
                    var def = ISA.GetIdef(wordAtAddr);

                    if (def.IsBranch)
                    {
                        var brOffset = (int)targetAddr - ((int)baseAddr + 1);
                        if (brOffset > MemoryImage.Length / 2)
                        {
                            brOffset -= MemoryImage.Length;
                        }
                        if (brOffset is > 127 or < -128)
                        {
                            throw new InvalidOpcodeException(ci.Def, "target address is too far away {offset}");
                        }

                        ci.AssembledWords[offset] = wordAtAddr.SetLower(brOffset);
                    }
                    else if (def.Mnemonic == "lli")
                    {
                        wordAtAddr = wordAtAddr.SetLower(targetAddr);
                        ci.AssembledWords[offset] = wordAtAddr;
                    }
                    else if (def.Mnemonic == "lui")
                    {
                        wordAtAddr = wordAtAddr.SetLower(targetAddr >> 8);
                        ci.AssembledWords[offset] = wordAtAddr;
                    }
                    else
                    {
                        throw new InvalidOpcodeException(ci.Def, "cannot use label as operand");
                    }
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

    // TODO: change over to CstInstructions
    public PdbInfo GetPdb() => new()
    {
        Labels = labels,
        RegisterAliases = registerAliases,
        LineAddrMap = lineMap.Select(kvp => KeyValuePair.Create(kvp.Value, kvp.Key)).ToDictionary(),
    };
}