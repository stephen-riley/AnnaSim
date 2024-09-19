using System.Text.RegularExpressions;
using AnnaSim.Instructions;
using CommandLine;

namespace AnnaSim.AsmParsing;

public class Parser
{
    public bool TryParseLine(string line, out ICstComponent? piece)
    {
        if (line.Trim() == "")
        {
            piece = new BlankLine();
            return true;
        }
        else if (line.StartsWith('#'))
        {
            piece = new HeaderComment() { Comment = line[1..].Trim() };
            return true;
        }
        else if (Regex.IsMatch(line, @"^[ \t]+#"))
        {
            piece = new InlineComment() { Comment = line.Trim()[1..].Trim() };
            return true;
        }
        else
        {
            string? comment = null;
            string? label = null;
            string? mnemonic;
            string[] operands;
            var opcode = InstrOpcode.Unknown;

            var commentIndex = line.IndexOf('#');
            if (commentIndex >= 0)
            {
                comment = line[(commentIndex + 1)..].Trim();
                line = line[0..commentIndex];
            }

            var colonIndex = line.IndexOf(':');
            if (colonIndex >= 0)
            {
                label = line[0..colonIndex];
                line = line[(colonIndex + 1)..].Trim();
            }
            var x = line.Split(' ', '\t').Where(p => p != "").ToArray();
            mnemonic = x[0];
            operands = x[1..];

            var str = mnemonic.Replace('.', '_');
            if (Enum.TryParse<InstrOpcode>(str, ignoreCase: true, out var result))
            {
                opcode = result;
            }
            else
            {
                throw new InvalidOperationException($"InstrOpcode ToEnum failed on input \"{opcode}\"");
            }

            piece = new CstInstruction()
            {
                // Labels = label is not null ? [label] : [],
                Labels = label != null ? [label] : [],
                Comment = comment,
                Opcode = opcode,
                Operand1 = operands.Length > 0 ? operands[0] : null,
                Operand2 = operands.Length > 1 ? operands[1] : null,
                Operand3 = operands.Length > 2 ? operands[2] : null,
            };

            return true;
        }
    }
}