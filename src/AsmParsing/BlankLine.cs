namespace AnnaSim.AsmParsing;

public class BlankLine : ICstComponent
{
    public string Comment { get; set; } = "";

    public uint Line { get; set; }

    public void Render(StreamWriter writer, bool showDisassembly = false) => writer.WriteLine();
}