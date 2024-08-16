namespace AnnaSim.Assembler;

public class PdbInfo
{
    public Dictionary<string, uint> Labels { get; set; } = [];
    public Dictionary<string, string> RegisterAliases { get; set; } = [];
    public Dictionary<int, uint> LineAddrMap { get; set; } = [];
}