namespace AnnaSim.TinyC.Scheduler.Instructions;

public class ScheduledInstruction
{
    public string? Label { get; set; }
    public InstructionEnum Opcode { get; set; }
    public string? Operand1 { get; set; }
    public string? Operand2 { get; set; }
    public string? Operand3 { get; set; }

    public string? PreTrivia { get; set; } = "";
    public string? PostTrivia { get; set; } = "";
}