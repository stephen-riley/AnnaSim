namespace AnnaSim.AsmParsing;

public class HeaderComment : ICstComponent
{
    public string Comment { get; set; } = "";

    public void Render(StreamWriter writer) => writer.WriteLine($"# {Comment}");
}