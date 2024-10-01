using AnnaSim.AsmParsing;

namespace AnnaSim.Assembler;

public class PdbInfo
{
    public Dictionary<string, uint> Labels { get; set; } = [];
    public Dictionary<string, string> RegisterAliases { get; set; } = [];
    public Dictionary<uint, CstInstruction> AddrCstMap { get; set; } = [];
    public Dictionary<uint, CstInstruction> LineCstMap { get; set; } = [];
}