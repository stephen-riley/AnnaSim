namespace AnnaSim.AsmParsing;

public class LabelComponent : ICstComponent
{
    public string Label { get; set; } = "";

    public void Render(StreamWriter writer) { }
}