using AnnaSim.AsmParsing;
using AnnaSim.TinyC.Optimizer;

namespace AnnaSim.TinyC.Scheduler;

public class InstructionScheduler
{
    public Stack<CstInstruction> Instructions { get; internal set; } = new();

    public InstructionScheduler()
    {
        Instructions.Push(new CstInstruction());
    }

    public void BlankLine() => Schedule(new BlankLine());

    public void InlineComment(string comment)
    {
        if (!string.IsNullOrWhiteSpace(comment))
        {
            Schedule(new InlineComment { Comment = comment });
        }
    }

    public void HeaderComment(string comment) => Schedule(new HeaderComment { Comment = comment });

    public void Label(string label) => Schedule(new LabelComponent { Label = label });

    public void Schedule(ICstComponent piece)
    {
        // states of CurrentInstr:
        // * IsDefined == false: keep attaching trivia to LeadingTrivia, 
        //                       or copy instruction fields
        // * IsDefined == true && piece is BlankLine: add to TrailingTrivia
        // * IsDefined == true && anything else: queue the instruction,
        //                                       start a new one, jump to top

        // Console.WriteLine($"got {piece.GetType().Name}; top is {Instructions.Peek().GetType().Name}, IsDefined={Instructions.Peek().IsDefined}");

        var peeked = Instructions.Peek();

        if (peeked.IsDefined == true && piece is BlankLine)
        {
            if (peeked.TrailingTrivia.Any())
            {
                // we don't want multiple blank lines in a row
                if (peeked.TrailingTrivia.Last().GetType() != typeof(BlankLine))
                {
                    peeked.TrailingTrivia.Add(piece);
                }
            }
            else
            {
                peeked.TrailingTrivia.Add(piece);
            }
        }
        else if (peeked.IsDefined)
        {
            Instructions.Push(new CstInstruction());
            Schedule(piece);
        }
        else if (piece is CstInstruction si)
        {
            peeked.CopyInstructionDataFrom(si);
        }
        else if (piece is LabelComponent l)
        {
            peeked.Labels.Add(l.Label);
        }
        else
        {
            peeked.LeadingTrivia.Add(piece);
        }
    }

    public int Optimize(bool showOptimizationComments = false)
    {
        Console.Error.WriteLine($"OPT: instr count before: {Instructions.Count}");
        var opt = new Opt(Instructions.Reverse()) { AddComments = showOptimizationComments };
        var optimizations = opt.Run();
        Console.Error.WriteLine($"OPT: optimizaiton count: {optimizations}");
        Instructions = opt.Instructions;
        Console.Error.WriteLine($"OPT: instr count after:  {Instructions.Count}");
        Console.Error.WriteLine();

        return optimizations;
    }

    public void Render()
    {
        var writer = new StreamWriter(Console.OpenStandardOutput())
        {
            AutoFlush = true
        };
        var oldWriter = Console.Out;
        Console.SetOut(writer);
        Render(writer);
        Console.SetOut(oldWriter);
    }

    public void Render(StreamWriter writer)
    {
        foreach (var i in Instructions.Reverse())
        {
            i.Render(writer);
        }
    }

    public void RenderDisassembly()
    {
        var fw = new StreamWriter("/tmp/out.dasm");
        foreach (var i in Instructions.Reverse())
        {
            i.Render(fw, true);
        }
    }
}