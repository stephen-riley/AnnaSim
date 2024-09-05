namespace AnnaSim.TinyC.Scheduler;

public interface IInstructionComponent
{
    public const int LabelColLength = 12;
    public const int OpcodeColLength = 8;
    public const int OperandColLength = 20;

    void Render(StreamWriter writer);
}