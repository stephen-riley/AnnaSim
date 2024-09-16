namespace AnnaSim.AsmParsing;

public class InlineComment : ICstComponent
{
    public string Comment { get; set; } = "";

    public void Render(StreamWriter writer) => writer.WriteLine(new string(' ', ICstComponent.LabelColLength) + $"# {Comment}");
}