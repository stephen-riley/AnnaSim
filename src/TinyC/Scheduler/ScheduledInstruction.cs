namespace AnnaSim.TinyC.Scheduler.Instructions;

/// <summary>
/// ScheduledInstructions are here so we can do quick peephole optimizations,
/// necessary since we're largely doing operations using a stack machine
/// (each push and pop of which requires two ANNA instructions).  When the
/// compiler is done emitting instructions, there will be a single list of
/// instructions *and only instructions*: all comments will be attached
/// to an instruction for later rendering.  In this manner, the optimizer
/// doesn't have to mess with looking at two instructions separated by an 
/// arbitrary number of comment lines and blank lines.  (This is inspired
/// by the Roslyn compiler's handling of source code so that Roslyn Code
/// Fixes tend to follow the original source code's style and structure.)
/// 
/// A ScheduledInstruction can have LeadingTrivia in the form of BlankLines,
/// HeaderComments (left-justified comments), and InlineComments (comments
/// indented to line up with opcodes).  ScheduledInstructions can also have
/// a comment on the same line, as well as TrailingTrivia.  The Scheduler
/// will rack up trivia until it sees a ScheduledInstruction, at which
/// point the accumulated trivia will be attached to the instruction as
/// LeadingTrivia.  Any subsequent BlankLines will be attached as
/// TraiingTrivia.  As soon as we see anything else, a new instruction
/// is started.
/// </summary>

public class ScheduledInstruction : IInstructionComponent
{
    public List<string> Labels { get; set; } = [];
    public InstrOpcode Opcode { get; set; }
    public string? Operand1 { get; set; }
    public string? Operand2 { get; set; }
    public string? Operand3 { get; set; }
    public string? Comment { get; set; }
    public List<IInstructionComponent> LeadingTrivia { get; set; } = [];
    public List<IInstructionComponent> TrailingTrivia { get; set; } = [];

    public IEnumerable<string> Operands { get => new List<string?> { Operand1, Operand2, Operand3 }.Cast<string>(); }

    public bool IsDefined { get => Opcode != InstrOpcode.Unknown; }

    public void Deconstruct(out string[] labels, out InstrOpcode opcode, out string? op1, out string? op2, out string? op3)
    {
        labels = [.. Labels];
        opcode = Opcode;
        op1 = Operand1;
        op2 = Operand2;
        op3 = Operand3;
    }

    public void CopyInstructionDataFrom(ScheduledInstruction? s)
    {
        if (s is not null)
        {
            Labels = [.. Labels, .. s.Labels];
            Opcode = s.Opcode;
            Operand1 = s.Operand1;
            Operand2 = s.Operand2;
            Operand3 = s.Operand3;
            Comment = s.Comment;

            // We specifically do *not* copy trivia here.
            // This method only handles the core data
            //  of an instruction.
        }
        else
        {
            throw new InvalidOperationException("cannot copy ScheduledInstruction from null");
        }
    }

    public void Render(StreamWriter writer)
    {
        LeadingTrivia.ForEach(t => t.Render(writer));

        var commentTerm = Comment is not null ? $"# {Comment}" : "";
        var labelTerm = Labels.Count > 0 ? $"{Labels[^1] + ':',-12}" : new string(' ', 12);

        var opTerm = $"{Opcode.ToString().ToLower().Replace('_', '.'),-8}";

        foreach (var l in Labels.Take(Labels.Count - 1))
        {
            writer.WriteLine($"{l}:");
        }

        writer.WriteLine($"{labelTerm}{opTerm}{string.Join(' ', Operands),-20}{commentTerm}");

        TrailingTrivia.ForEach(t => t.Render(writer));
    }
}