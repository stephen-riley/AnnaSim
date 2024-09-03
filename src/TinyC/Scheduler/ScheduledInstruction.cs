namespace AnnaSim.TinyC.Scheduler.Instructions;

public class ScheduledInstruction
{
    public string? Label { get; set; }
    public InstrOpcode Opcode { get; set; }
    public string? Operand1 { get; set; }
    public string? Operand2 { get; set; }
    public string? Operand3 { get; set; }

    public string? PreTrivia { get; set; } = "";
    public string? PostTrivia { get; set; } = "";

    public void Deconstruct(out string? label, out InstrOpcode opcode, out string? op1, out string? op2, out string? op3)
    {
        label = Label;
        opcode = Opcode;
        op1 = Operand1;
        op2 = Operand2;
        op3 = Operand3;
    }
}