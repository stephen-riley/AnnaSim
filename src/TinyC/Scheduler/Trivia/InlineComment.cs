using AnnaSim.TinyC.Scheduler.Instructions;

namespace AnnaSim.TinyC.Scheduler;

public class InlineComment : IRenderInstruction
{
    public string Comment { get; set; } = "";

    public void Render(StreamWriter writer)
    {
        throw new NotImplementedException();
    }
}