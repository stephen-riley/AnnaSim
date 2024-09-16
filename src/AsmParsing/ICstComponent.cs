namespace AnnaSim.AsmParsing;

public interface ICstComponent
{
    public const int LabelColLength = 14;
    public const int OpcodeColLength = 8;
    public const int OperandColLength = 20;

    void Render(StreamWriter writer);
}