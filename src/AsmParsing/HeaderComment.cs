namespace AnnaSim.AsmParsing;

public class HeaderComment : ICstComponent
{
    public string Comment { get; set; } = "";

    public int Line { get; set; }

    public void Render(StreamWriter writer, bool showDisassembly = false)
        => writer.WriteLine($"{(showDisassembly ? new string(' ', ICstComponent.BitColLength) : "")}# {Comment}");
}