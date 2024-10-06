using AnnaSim.Cpu.Memory;
using AnnaSim.Extensions;

namespace AnnaSim.AsmParsing;

public class CstProgram
{
    public List<CstInstruction> Instructions { get; set; } = [];
    public MemoryFile? MemoryImage { get; set; }

    public Dictionary<uint, CstInstruction> LineMap { get; internal set; } = [];
    public Dictionary<uint, CstInstruction> AddrMap { get; internal set; } = [];
    public Dictionary<string, uint> Labels { get; set; } = [];
    public Dictionary<string, string> RegisterAliases { get; set; } = [];

    public string? Filename { get; set; }

    public CstProgram(IEnumerable<CstInstruction> instructions)
    {
        Instructions = instructions.ToList();
        BuildMaps();
    }

    private void BuildMaps()
    {
        Instructions.ForEach(ci => LineMap[ci.Line] = ci);

        foreach (var ci in Instructions.ThatOccupyMemory())
        {
            ci.AssembledWords.SelectWithIndex().ForEach(tuple => AddrMap[ci.BaseAddress + (uint)tuple.index] = ci);
        }
    }
}