namespace AnnaSim.Assembler;

public class FrameDef
{
    public string Name { get; set; } = null!;
    public SortedDictionary<int, string> Members { get; set; } = null!;

    public uint StartAddr { get; set; }
    public uint EndAddr { get; set; }

    public FrameDef(string name)
    {
        Name = name;
        Members = [];
    }

    public FrameDef AddMember(int index, string name)
    {
        Members[index] = name;
        return this;
    }
}