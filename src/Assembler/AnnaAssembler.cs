using System.Text.RegularExpressions;
using AnnaSim.Instructions;
using AnnaSim.Cpu.Memory;
using AnnaSim.Exceptions;

namespace AnnaSim.Assembler;

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
        InstructionDefinition.SetAssembler(this);
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
            var pieces = new Regex(@"\s+").Split(line).Where(s => s != "").ToArray();

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

        if (I.Lookup.TryGetValue(pieces[idx], out var def))
        {
            def.Asm = this;
            def.Assemble(pieces[idx..].Select(s => Operand.Parse(s)).ToArray());
        }
    }

    internal void ResolveLabels()
    {
        foreach ((var addr, var label) in resolutionToDo)
        {
            if (labels.TryGetValue(label, out var targetAddr))
            {
                var wordAtAddr = MemoryImage[addr];
                var def = I.GetIdef(wordAtAddr);

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
}