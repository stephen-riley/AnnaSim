using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using AnnaSim.Exceptions;
using AnnaSim.Instructions;

namespace AnnaSim.AsmParsing;

public static class CstParser
{
    private enum ParseState
    {
        BeforeInstruction,
        AfterInstruction,
    };

    public static List<CstInstruction> ParseFile(string filename)
        => ParseLines(File.ReadAllLines(filename));

    public static List<CstInstruction> ParseSource(string source)
        => ParseLines(source.Split("\n"));

    // We're going to collect all trivia and attach it to the first instruction we see.
    // Thereafter, collect all blank lines after an instruction.  When we see something
    // that is *not* a blank line, we start collecting all trivia until the next
    // instruction.  Lather, rinse, repeat.
    //
    // Also, there are labels. (Explaination TODO)
    public static List<CstInstruction> ParseLines(IEnumerable<string> lines)
    {
        var instructions = new Stack<CstInstruction>();
        var trivia = new List<ICstComponent>();
        var labels = new List<LabelComponent>();

        var state = ParseState.BeforeInstruction;

        uint lineNumber = 0;

        foreach (var line in lines)
        {
            lineNumber++;

            try
            {
                if (TryParseLine(line, out var piece, lineNumber))
                {
                    if (state == ParseState.BeforeInstruction)
                    {
                        if (piece is CstInstruction ci)
                        {
                            ci.LeadingTrivia.AddRange(trivia);
                            ci.Labels.InsertRange(0, labels.Select(l => l.Label));
                            instructions.Push(ci);
                            trivia.Clear();
                            labels.Clear();
                            state = ParseState.AfterInstruction;
                        }
                        else if (piece is LabelComponent l)
                        {
                            labels.Add(l);
                            // state remains BeforeInstruction
                        }
                        else
                        {
                            trivia.Add(piece);
                            // state remains BeforeInstruction
                        }
                    }
                    else
                    {
                        if (piece is BlankLine)
                        {
                            instructions.Peek().TrailingTrivia.Add(piece);
                            // state remains AfterInstruction
                        }
                        else if (piece is CstInstruction ci)
                        {
                            instructions.Push(ci);
                            // state remains AfterInstruction
                        }
                        else if (piece is LabelComponent l)
                        {
                            labels.Add(l);
                            state = ParseState.BeforeInstruction;
                        }
                        else
                        {
                            trivia.Add(piece);
                            state = ParseState.BeforeInstruction;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new AssemblerParseException(line, e);
            }
        }

        if (trivia.Count > 0 && instructions.Count > 0)
        {
            instructions.Peek().TrailingTrivia.AddRange(trivia);
        }

        return instructions.Reverse().ToList();
    }

    public static bool TryParseLine(string originalLine, [NotNullWhen(true)] out ICstComponent? piece, uint lineNumber = 0)
    {
        var line = new string(originalLine);

        if (line.Trim() == "")
        {
            piece = new BlankLine() { Line = lineNumber };
            return true;
        }
        else if (line.StartsWith('#'))
        {
            piece = new HeaderComment() { Comment = line[1..].Trim(), Line = lineNumber };
            return true;
        }
        else if (Regex.IsMatch(line, @"^[ \t]+#"))
        {
            piece = new InlineComment() { Comment = line.Trim()[1..].Trim(), Line = lineNumber };
            return true;
        }
        else if (Regex.IsMatch(line.Trim(), @"^[A-Za-z0-9_]+:$"))
        {
            piece = new LabelComponent() { Label = line[0..^1], Line = lineNumber };
            return true;
        }
        else
        {
            string? comment = null;
            string? label = null;
            string? mnemonic;
            string[] operands;
            var opcode = InstrOpcode.Unknown;

            line = line.Trim();

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

            // At this point we've removed any labels and comments from the
            //  line.  All that's left now is the mnemonic and operands.
            // At this point in the assembly language, a string operand
            //  can be the only operand for the directive, so either we
            //  see a double quote and take the rest of the line as the
            //  string operand, or we split on spaces. 
            var x = line.Split(' ', '\t').Where(p => p != "").ToArray();
            mnemonic = x[0];
            if (x.Length > 1)
            {
                if (x[1].StartsWith('"'))
                {
                    operands = [string.Join(' ', x[1..])];
                }
                else
                {
                    operands = x[1..];
                }
            }
            else
            {
                operands = [];
            }

            var str = mnemonic.Replace('.', '_');
            if (Enum.TryParse<InstrOpcode>(str, ignoreCase: true, out var result))
            {
                opcode = result;
            }
            else
            {
                throw new AssemblerParseException($"InstrOpcode ToEnum failed on input \"{str}\"");
            }

            piece = new CstInstruction()
            {
                Labels = label != null ? [label] : [],
                Comment = comment,
                Mnemonic = x[0],
                Opcode = opcode,
                OperandStrings = operands,
                Line = lineNumber
            };

            return true;
        }
    }
}