namespace AnnaSim.AsmParsing;

public class LabelComponent : ICstComponent
{
    public string Label { get; set; } = "";

    public int Line { get; set; }

    public void Render(StreamWriter writer) => writer.WriteLine(Label + ':');
}