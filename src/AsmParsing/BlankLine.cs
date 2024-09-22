namespace AnnaSim.AsmParsing;

public class BlankLine : ICstComponent
{
    public string Comment { get; set; } = "";

    public int Line { get; set; }

    public void Render(StreamWriter writer) => writer.WriteLine();
}