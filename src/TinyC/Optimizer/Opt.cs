using AnnaSim.AsmParsing;

using static AnnaSim.Instructions.InstrOpcode;

namespace AnnaSim.TinyC.Optimizer;

public class Opt
{
    public Stack<CstInstruction> Instructions { get; internal set; } = new();

    private Stack<CstInstruction> pass = null!;

    internal List<Func<int>> Optimizers = [];

    public bool AddComments { get; set; } = false;

    public Opt(IEnumerable<CstInstruction> instr)
    {
        foreach (var i in instr)
        {
            Instructions.Push(i);
        }
        Optimizers.AddRange([OptPushPop, OptBranchToNextByLabel, OptBranchToNextByOffset, OptBackToBackJumps, OptLoadMov, OptRedundantVarStoreLoadLwi]);
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

                pass.Push(new CstInstruction
                {
                    LeadingTrivia = leadingTrivia,
                    Labels = [.. prevLabels, .. curLabels],
                    Opcode = Mov,
                    OperandStrings = [
                        curOp2 ?? throw new InvalidOperationException($"{nameof(curOp2)} is null"),
                        prevOp2 ?? throw new InvalidOperationException($"{nameof(prevOp2)} is null")
                    ],
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
                        prev.LeadingTrivia.Add(new InlineComment { Comment = $"OPTIMIZATION: removed reduntant beq r0 {curOp2}" });
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

    internal int OptLoadMov()
    {
        // l*i  r3 2    # load constant 2 -> r3
        // mov  r2 r3   # transfer r3 to r2
        //  to:
        // l*i  r2 2

        var cur = pass.Pop();
        var prev = pass.Pop();

        var (prevLabels, prevOpcode, prevOp1, prevOp2, _) = prev;
        var (curLabels, curOpcode, curOp1, curOp2, _) = cur;

        if (prevOpcode is Lli or Lwi && prevOp1 is not null && prevOp2 is not null)
        {
            if (curOpcode is Mov && curOp1 is not null)
            {
                if (prevOp1 == curOp2)
                {
                    var leadingTrivia = prev.LeadingTrivia;
                    if (AddComments)
                    {
                        leadingTrivia = [.. leadingTrivia, new InlineComment { Comment = $"OPTIMIZATION: removed unnecessary mov and loaded directly to target register" }];
                    }
                    pass.Push(new CstInstruction
                    {
                        LeadingTrivia = leadingTrivia,
                        Labels = [.. prevLabels, .. curLabels],
                        Opcode = prevOpcode,
                        OperandStrings = [curOp1, prevOp2],
                        Comment = $"transfer {prevOp2} to {curOp2}",
                        TrailingTrivia = cur.TrailingTrivia
                    });
                    return 1;
                }
            }
        }

        pass.Push(prev);
        pass.Push(cur);
        return 0;
    }

    internal int OptRedundantVarStoreLoadLwi()
    {
        // lwi     r1 &_var_b          # load address of variable "b"
        // sw      r3 r1 0             # store variable "b" to data segment
        // lwi     r1 &_var_b          # load address of variable b
        // lw      r3 r1 0             # load variable "b" from data segment
        //  to:
        // lwi     r1 &_var_b          # load address of variable "b"
        // sw      r3 r1 0             # store variable "b" to data segment
        // # r3 still has the original value

        if (pass.Count() < 4)
        {
            return 0;
        }

        var i4 = pass.Pop();
        var i3 = pass.Pop();
        var i2 = pass.Pop();
        var i1 = pass.Pop();

        var instrMatch = i1.Opcode == Lwi && i2.Opcode == Sw && i3.Opcode == Lwi && i4.Opcode == Lw;
        var operandMatch = Enumerable.SequenceEqual(i1.Operands, i3.Operands) && Enumerable.SequenceEqual(i2.Operands, i4.Operands);
        if (instrMatch && operandMatch)
        {
            var leadingTrivia = i1.LeadingTrivia;
            if (AddComments)
            {
                leadingTrivia = [.. leadingTrivia, new InlineComment { Comment = "OPTIMIZATION: removed redundant lw" }];
            }
            pass.Push(new CstInstruction
            {
                LeadingTrivia = leadingTrivia,
                Labels = i1.Labels,
                Opcode = i1.Opcode,
                OperandStrings = i1.OperandStrings,
                Comment = i1.Comment,
                TrailingTrivia = i1.TrailingTrivia
            });
            pass.Push(i2);
            return 2;
        }

        pass.Push(i1);
        pass.Push(i2);
        pass.Push(i3);
        pass.Push(i4);
        return 0;
    }

    internal int OptRedundantVarStoreLoadLliLui()
    {
        throw new NotImplementedException();
    }
}