namespace AnnaSim.AsmParsing;

public class InlineComment : ICstComponent
{
    public string Comment { get; set; } = "";

    public uint Line { get; set; }

    public void Render(StreamWriter writer, bool showDisassembly = false)
        => writer.WriteLine(
            (showDisassembly ? new string(' ', ICstComponent.BitColLength) : "")
                + new string(' ', ICstComponent.LabelColLength) + $"# {Comment}");
}