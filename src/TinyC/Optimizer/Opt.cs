using AnnaSim.Extensions;
using AnnaSim.TinyC.Scheduler.Components;
using AnnaSim.TinyC.Scheduler.Instructions;

using static AnnaSim.TinyC.Scheduler.Instructions.InstrOpcode;

namespace AnnaSim.TinyC.Optimizer;

public class Opt
{
    public Stack<ScheduledInstruction> Instructions { get; internal set; } = new();

    private Stack<ScheduledInstruction> pass = null!;

    internal List<Func<int>> Optimizers = [];

    public Opt(IEnumerable<ScheduledInstruction> instr)
    {
        foreach (var i in instr)
        {
            Instructions.Push(i);
        }
        Optimizers.AddRange([OptPushPop, OptBranchToNext]);
    }

    public int Run()
    {
        var total = 0;
        int count;
        do
        {
            count = RunPass();
            total += count;
        } while (count > 0);

        return total;
    }

    internal int RunPass()
    {
        if (Instructions.Count == 0)
        {
            return 0;
        }

        int optimizations = 0;

        pass = new();
        var oldStack = Instructions.Reverse();
        pass.Push(oldStack.Take(1).First());

        foreach (var i in oldStack.Skip(1))
        {
            pass.Push(i);
            Optimizers.ForEach(o =>
            {
                if (pass.Count >= 2)
                {
                    optimizations += o.Invoke();
                }
            });
        }

        Instructions = pass;

        return optimizations;
    }

    // TODO: back to back beq r0's
    // TODO: beq to very next instruction

    internal int OptBranchToNext()
    {
        var cur = pass.Pop();
        var prev = pass.Pop();

        var (curLabels, _, _, _, _) = cur;
        var (_, prevOpcode, prevOp1, prevOp2, _) = prev;

        if (prevOpcode == Beq && prevOp1 == "r0")
        {
            if (prevOp2 is not null && prevOp2.StartsWith('&'))
            {
                if (curLabels.Contains(prevOp2[1..]))
                {
                    cur.LeadingTrivia.Add(new InlineComment { Comment = $"OPTIMIZATION: removed unnecessary beq r0 {prevOp2}" });
                    pass.Push(cur);
                    return 1;
                }
            }
        }

        pass.Push(prev);
        pass.Push(cur);
        return 0;
    }

    internal int OptPushPop()
    {
        var cur = pass.Pop();
        var prev = pass.Pop();

        var (curLabels, curOpcode, curOp1, curOp2, _) = cur;
        var (prevLabels, prevOpcode, prevOp1, prevOp2, _) = prev;

        // if rd and rs1 is the same for both instructions, then delete them both.
        //  otherwise...
        if (prevOpcode == Push && curOpcode == Pop && curOp1 == prevOp1)
        {
            if (curOp2 != prevOp2)
            {
                pass.Push(new ScheduledInstruction
                {
                    LeadingTrivia = [.. prev.LeadingTrivia, new InlineComment { Comment = "OPTIMIZATION: push/pop becomes mov" }],
                    Labels = [.. prevLabels, .. curLabels],
                    Opcode = Mov,
                    Operand1 = curOp2,
                    Operand2 = prevOp2,
                    Comment = $"transfer {prevOp2} to {curOp2}",
                    TrailingTrivia = cur.TrailingTrivia
                });
            }
            else
            {
                pass.Peek().TrailingTrivia.Add(new InlineComment { Comment = $"OPTIMIZATION: canceled push/pop on {curOp2}" });
                // we can safely remove both push and pop
            }

            return 1;
        }
        else
        {
            pass.Push(prev);
            pass.Push(cur);
            return 0;
        }
    }
}