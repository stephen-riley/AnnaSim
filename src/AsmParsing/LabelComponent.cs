namespace AnnaSim.AsmParsing;

public class LabelComponent : ICstComponent
{
    public string Label { get; set; } = "";

    public int Line { get; set; }

    public void Render(StreamWriter writer, bool showDisassembly = false)
        => writer.WriteLine((showDisassembly ? new string(' ', ICstComponent.BitColLength) : "") + Label + ':');
}