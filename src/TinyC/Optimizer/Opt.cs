using AnnaSim.TinyC.Scheduler.Components;
using AnnaSim.TinyC.Scheduler.Instructions;

using static AnnaSim.TinyC.Scheduler.Instructions.InstrOpcode;

namespace AnnaSim.TinyC.Optimizer;

public class Opt
{
    public Stack<ScheduledInstruction> Instructions { get; internal set; } = new();

    private Stack<ScheduledInstruction> pass = null!;

    internal List<Func<int>> Optimizers = [];

    public bool AddComments { get; set; } = false;

    public Opt(IEnumerable<ScheduledInstruction> instr)
    {
        foreach (var i in instr)
        {
            Instructions.Push(i);
        }
        Optimizers.AddRange([OptPushPop, OptBranchToNextByLabel, OptBranchToNextByOffset, OptBackToBackJumps]);
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

    internal int OptBranchToNextByLabel()
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
                    if (AddComments)
                    {
                        cur.LeadingTrivia.Add(new InlineComment { Comment = $"OPTIMIZATION: removed unnecessary beq r0 {prevOp2}" });
                    }
                    pass.Push(cur);
                    return 1;
                }
            }
        }

        pass.Push(prev);
        pass.Push(cur);
        return 0;
    }

    internal int OptBranchToNextByOffset()
    {
        var cur = pass.Pop();
        var prev = pass.Pop();

        var (curLabels, _, _, _, _) = cur;
        var (_, prevOpcode, prevOp1, prevOp2, _) = prev;

        if (prevOpcode == Beq && prevOp1 == "r0")
        {
            if (prevOp2 is not null && prevOp2 == "1")
            {
                if (AddComments)
                {
                    cur.LeadingTrivia.Add(new InlineComment { Comment = $"OPTIMIZATION: removed unnecessary beq r0 {prevOp2}" });
                }
                pass.Push(cur);
                return 1;
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
                var leadingTrivia = prev.LeadingTrivia;
                if (AddComments)
                {
                    leadingTrivia = [.. leadingTrivia, new InlineComment { Comment = "OPTIMIZATION: push/pop becomes mov" }];
                }

                pass.Push(new ScheduledInstruction
                {
                    LeadingTrivia = leadingTrivia,
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
                if (pass.Count > 0 && AddComments)
                {
                    pass.Peek().TrailingTrivia.Add(new InlineComment { Comment = $"OPTIMIZATION: canceled push/pop on {curOp2}" });
                }
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

    internal int OptBackToBackJumps()
    {
        var cur = pass.Pop();
        var prev = pass.Pop();

        // 
        var (curLabels, curOpcode, curOp1, curOp2, _) = cur;
        var (_, prevOpcode, prevOp1, _, _) = prev;

        // if the second `beq r0` has a label, it may be
        //  a jump target, so we can't get rid of it.
        if (curLabels.Length == 0)
        {
            if (prevOpcode == Beq && prevOp1 == "r0")
            {
                if (curOpcode == Beq && curOp1 == "r0")
                {
                    if (AddComments)
                    {
                        cur.LeadingTrivia.Add(new InlineComment { Comment = $"OPTIMIZATION: removed reduntant beq r0 {curOp2}" });
                    }
                    pass.Push(prev);
                    return 1;
                }
            }
        }

        pass.Push(prev);
        pass.Push(cur);
        return 0;
    }
}