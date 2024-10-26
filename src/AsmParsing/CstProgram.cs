using AnnaSim.Assembler;
using AnnaSim.Cpu.Memory;
using AnnaSim.Extensions;
using CommandLine;

namespace AnnaSim.AsmParsing;

public class CstProgram
{
    public List<CstInstruction> Instructions { get; set; } = [];
    public MemoryFile? MemoryImage { get; set; }

    public Dictionary<uint, CstInstruction> LineMap { get; internal set; } = [];
    public Dictionary<uint, CstInstruction> AddrMap { get; internal set; } = [];
    public Dictionary<string, uint> Labels { get; set; } = [];
    public Dictionary<string, string> RegisterAliases { get; set; } = [];
    public Dictionary<string, FrameDef> FrameDefs { get; internal set; } = [];
    public Dictionary<Range, FrameDef> FrameAddrMap { get; internal set; } = [];

    public string? Filename { get; set; }

    public CstProgram(IEnumerable<CstInstruction> instructions, IEnumerable<FrameDef> frameDefs)
    {
        Instructions = instructions.ToList();
        BuildMaps();
        ExtractLabels();
        BuildFrameAddrMap(frameDefs);
    }

    private void BuildMaps()
    {
        Instructions.ForEach(ci => LineMap[ci.Line] = ci);

        foreach (var ci in Instructions.ThatOccupyMemory())
        {
            ci.AssembledWords.SelectWithIndex().ForEach(tuple => AddrMap[ci.BaseAddress + (uint)tuple.index] = ci);
        }
    }

    private void ExtractLabels()
    {
        foreach (var ci in Instructions)
        {
            foreach (var label in ci.Labels)
            {
                Labels[label] = ci.BaseAddress;
            }
        }
    }

    private void BuildFrameAddrMap(IEnumerable<FrameDef> frameDefs)
    {
        foreach (var f in frameDefs)
        {
            FrameAddrMap[(int)f.StartAddr..(int)f.EndAddr] = f;
        }
    }
}