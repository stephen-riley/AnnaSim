namespace AnnaSim.AsmParsing;

public interface ICstComponent
{
    public const int BitColLength = 14;
    public const int LabelColLength = 14;
    public const int OpcodeColLength = 8;
    public const int OperandColLength = 20;

    public int Line { get; set; }

    void Render(StreamWriter writer, bool showDisassembly);
}