using System.Text.RegularExpressions;
using AnnaSim.Assembler;
using AnnaSim.Cpu.Memory;
using AnnaSim.Extensions;
using AnnaSim.Instructions;

namespace AnnaSim.AsmParsing;

/// <summary>
/// CstInstructions are here so we can do quick peephole optimizations,
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
/// A CstInstruction can have LeadingTrivia in the form of BlankLines,
/// HeaderComments (left-justified comments), and InlineComments (comments
/// indented to line up with opcodes).  CstInstructions can also have
/// a comment on the same line, as well as TrailingTrivia.  The Scheduler
/// will rack up trivia until it sees a CstInstruction, at which
/// point the accumulated trivia will be attached to the instruction as
/// LeadingTrivia.  Any subsequent BlankLines will be attached as
/// TraiingTrivia.  As soon as we see anything else, a new instruction
/// is started.
/// </summary>

public class CstInstruction : ICstComponent
{
    public List<string> Labels { get; set; } = [];
    public string Mnemonic { get; set; } = null!;
    public InstrOpcode Opcode { get; set; }
    public List<ICstComponent> LeadingTrivia { get; set; } = [];
    public List<ICstComponent> TrailingTrivia { get; set; } = [];

    public InstructionDefinition Def { get; set; } = null!;

    private Operand[] cachedOperands = null!;
    public Operand[] Operands
    {
        get
        {
            cachedOperands ??= OperandStrings.Select(s => Operand.Parse(s)).ToArray();
            return cachedOperands;
        }
    }

    private string[] cachedOperandStrings = null!;
    public string[] OperandStrings
    {
        get => cachedOperandStrings;
        set
        {
            cachedOperandStrings = value;
            cachedOperands = null!;
        }
    }

    public string? Operand1 { get => OperandStrings.Length > 0 ? OperandStrings[0] : null; }
    public string? Operand2 { get => OperandStrings.Length > 1 ? OperandStrings[1] : null; }
    public string? Operand3 { get => OperandStrings.Length > 2 ? OperandStrings[2] : null; }
    public string? Comment { get; set; }

    public uint BaseAddress { get; set; }
    public List<Word> AssembledWords { get; set; } = [];

    public uint Line { get; set; }

    public CstInstruction() { }

    public CstInstruction(string? label, InstrOpcode opcode, string? op1, string? op2 = null, string? op3 = null, string? comment = null)
    {
        Labels = label is not null ? [label] : Labels;
        Mnemonic = opcode.ToString().Replace('_', '.');
        Opcode = opcode;
        OperandStrings = new List<string?>() { op1, op2, op3 }.Cast<string>().ToArray();
        Comment = comment;
    }

    public CstInstruction(string? label, string mnemonic, InstrOpcode opcode, string? op1, string? op2 = null, string? op3 = null, string? comment = null)
    {
        Labels = label is not null ? [label] : Labels;
        Mnemonic = mnemonic;
        Opcode = opcode;
        OperandStrings = new List<string?>() { op1, op2, op3 }.Cast<string>().ToArray();
        Comment = comment;
    }

    public bool IsDefined { get => Opcode != InstrOpcode.Unknown; }

    public void Deconstruct(out string[] labels, out InstrOpcode opcode, out string? op1, out string? op2, out string? op3)
    {
        labels = [.. Labels];
        opcode = Opcode;
        op1 = Operand1;
        op2 = Operand2;
        op3 = Operand3;
    }

    public void CopyInstructionDataFrom(CstInstruction? s)
    {
        if (s is not null)
        {
            Labels = [.. Labels, .. s.Labels];
            Opcode = s.Opcode;
            OperandStrings = (string[])s.OperandStrings.Clone();
            Comment = s.Comment;

            // We specifically do *not* copy trivia here.
            // This method only handles the core data
            //  of an instruction.
        }
        else
        {
            throw new InvalidOperationException("cannot copy CstInstruction from null");
        }
    }

    public uint AssignBits(uint addr, params Word[] bits)
    {
        BaseAddress = addr;
        AssembledWords.AddRange(bits);
        return addr + (uint)bits.Length;
    }

    public void Render(StreamWriter writer, bool showDisassembly = false)
    {
        LeadingTrivia.ForEach(t => t.Render(writer, showDisassembly));

        var commentTerm = Comment is not null ? $"# {Comment}" : "";
        var labelTerm = Labels.Count > 0 ? $"{Labels[^1] + ':',-ICstComponent.LabelColLength}" : new string(' ', ICstComponent.LabelColLength);

        var xOpcode = Opcode.ToString().ToLower().Replace('_', '.');
        var opTerm = $"{xOpcode,-ICstComponent.OpcodeColLength}";

        foreach (var l in Labels.Take(Labels.Count - 1))
        {
            writer.WriteLine($"{(showDisassembly ? new string(' ', ICstComponent.BitColLength) : "")}{l}:");
        }

        var firstBitsTerm = "";
        List<string> bits = [];
        if (showDisassembly)
        {
            var addr = BaseAddress;
            bits = AssembledWords.Select(w => $"[{addr++:x4}: {(uint)w:x4}]").ToList();
            firstBitsTerm = bits.Count > 0 ? $"{bits[0],-ICstComponent.BitColLength}" : new string(' ', ICstComponent.BitColLength);
        }

        writer.WriteLine($"{firstBitsTerm}{labelTerm}{opTerm}{RenderOperands(xOpcode, OperandStrings),-ICstComponent.OperandColLength}{commentTerm}");

        if (showDisassembly && bits.Count > 0)
        {
            foreach (var b in bits[1..])
            {
                writer.WriteLine(b);
            }
        }

        TrailingTrivia.ForEach(t => t.Render(writer, showDisassembly));
    }

    public string RenderSimpleInstruction(ref int maxLabelLength, ref int maxOpcodeOperandLength, ref int maxInstructionLength)
    {
        var labelTerm = "";
        if (Labels.Count > 0)
        {
            var label = Labels[^1] + ": ";
            maxLabelLength = int.Max(label.Length, maxLabelLength);
            labelTerm = label.ToWidth(maxLabelLength);
        }
        else
        {
            labelTerm = labelTerm.ToWidth(maxLabelLength);
        }

        var xOpcode = Opcode.ToString().ToLower().Replace('_', '.');
        var opTerm = $"{xOpcode}".ToWidth(ICstComponent.OpcodeColLength);
        var operands = RenderOperands(xOpcode, OperandStrings);
        var opcodeOperandsTerm = $"{opTerm}{operands}".ToWidth(maxOpcodeOperandLength);
        maxOpcodeOperandLength = int.Max(opcodeOperandsTerm.Length, maxOpcodeOperandLength);

        var instrTerm = $"{labelTerm}{opcodeOperandsTerm}".ToWidth(maxOpcodeOperandLength);
        maxInstructionLength = int.Max(instrTerm.Length, maxInstructionLength);

        var bits = $"[{BaseAddress:x4}: {AssembledWords[0]:x4}]";

        return $"{bits} {instrTerm}";
    }

    // renders register offsets as expressions if Rs1 is an alias
    private static string RenderOperands(string opcode, string[] operands)
    {
        if (opcode is "lw" or "sw" or "addi")
        {
            var op1IsPlainRegister = operands[1].StartsWith('r') && operands[1].Length == 2 && operands[1][1] is >= '0' and <= '7';
            if (!op1IsPlainRegister && int.TryParse(operands[2], out int rs2))
            {
                return $"{operands[0]} {operands[1]}{(rs2 >= 0 ? "+" : "")}{operands[2]}";
            }
        }

        return string.Join(' ', operands);
    }
}