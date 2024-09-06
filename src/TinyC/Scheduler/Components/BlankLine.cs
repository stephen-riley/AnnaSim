namespace AnnaSim.TinyC.Scheduler.Components;

public class BlankLine : IInstructionComponent
{
    public string Comment { get; set; } = "";

    public void Render(StreamWriter writer) => writer.WriteLine();
}