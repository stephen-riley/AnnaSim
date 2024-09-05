using AnnaSim.TinyC.Scheduler.Instructions;

using static AnnaSim.TinyC.Scheduler.Instructions.InstrOpcode;

namespace AnnaSim.TinyC.Scheduler;

public class InstructionScheduler
{
    public Stack<ScheduledInstruction> Instructions { get; } = new();

    internal List<Func<bool>> Optimizers = [];

    public InstructionScheduler()
    {
        Instructions.Push(new ScheduledInstruction());

        Optimizers.Add(OptPushPop);
    }

    public void BlankLine() => Schedule(new BlankLine());

    public void InlineComment(string comment) => Schedule(new InlineComment { Comment = comment });

    public void HeaderComment(string comment) => Schedule(new HeaderComment { Comment = comment });

    public void Label(string label) => Instructions.Peek().Labels.Add(label);

    public void Schedule(IInstructionComponent piece)
    {
        // states of CurrentInstr:
        // * IsDefined == false: keep attaching trivia to LeadingTrivia, 
        //                       or copy instruction fields
        // * IsDefined == true && piece is BlankLine: add to TrailingTrivia
        // * IsDefined == true && anything else: queue the instruction,
        //                                       start a new one, jump to top

        // Console.WriteLine($"got {piece.GetType().Name}; top is {Instructions.Peek().GetType().Name}, IsDefined={Instructions.Peek().IsDefined}");

        if (Instructions.Peek().IsDefined == true && piece is BlankLine)
        {
            Instructions.Peek().TrailingTrivia.Add(piece);
            return;
        }
        else if (Instructions.Peek().IsDefined)
        {
            Instructions.Push(new ScheduledInstruction());
            Schedule(piece);
        }
        else if (piece is ScheduledInstruction)
        {
            Instructions.Peek().CopyInstructionDataFrom(piece as ScheduledInstruction);
        }
        else
        {
            Instructions.Peek().LeadingTrivia.Add(piece);
        }
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

    internal bool OptPushPop()
    {
        var cur = Instructions.Pop();
        var prev = Instructions.Pop();

        var (curLabels, curOpcode, curOp1, curOp2, _) = cur;
        var (prevLabels, prevOpcode, prevOp1, prevOp2, _) = prev;

        // if rd and rs1 is the same for both instructions, then delete them both.
        //  otherwise...
        if (curOpcode == Pop && prevOpcode == Push && curOp1 == prevOp1)
        {
            if (curOp2 != prevOp2)
            {
                Instructions.Push(new ScheduledInstruction
                {
                    LeadingTrivia = prev.LeadingTrivia,
                    Labels = [.. prevLabels, .. curLabels],
                    Opcode = Mov,
                    Operand1 = curOp2,
                    Operand2 = prevOp2,
                    TrailingTrivia = cur.TrailingTrivia
                });
            }

            return true;
        }
        else
        {
            Instructions.Push(prev);
            Instructions.Push(cur);
            return false;
        }
    }
}