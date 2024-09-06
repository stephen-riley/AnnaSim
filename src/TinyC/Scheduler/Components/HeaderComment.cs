namespace AnnaSim.TinyC.Scheduler.Components;

public class HeaderComment : IInstructionComponent
{
    public string Comment { get; set; } = "";

    public void Render(StreamWriter writer) => writer.WriteLine($"# {Comment}");
}