namespace AnnaSim.TinyC.Scheduler.Components;

public class LabelComponent : IInstructionComponent
{
    public string Label { get; set; } = "";

    public void Render(StreamWriter writer) { }
}