using AnnaSim.TinyC.Scheduler.Instructions;

namespace AnnaSim.TinyC.Scheduler;

public class HeaderComment : IInstructionComponent
{
    public string Comment { get; set; } = "";

    public void Render(StreamWriter writer) => writer.WriteLine($"# {Comment}");
}