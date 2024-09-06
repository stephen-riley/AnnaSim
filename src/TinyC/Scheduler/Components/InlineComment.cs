namespace AnnaSim.TinyC.Scheduler.Components;

public class InlineComment : IInstructionComponent
{
    public string Comment { get; set; } = "";

    public void Render(StreamWriter writer) => writer.WriteLine(new string(' ', 12) + $"# {Comment}");
}