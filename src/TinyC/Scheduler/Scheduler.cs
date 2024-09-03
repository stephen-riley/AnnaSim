using AnnaSim.Instructions;
using AnnaSim.TinyC.Scheduler.Instructions;

using static AnnaSim.TinyC.Scheduler.Instructions.InstrOpcode;

namespace AnnaSim.TinyC.Scheduler;

public static class Scheduler
{
    public static List<ScheduledInstruction> Instructions { get; } = [];
    public static Stack<ScheduledInstruction> CurrentWindow { get; } = [];

    internal static List<Func<bool>> Optimizers = [
        OptPushPop
    ];

    public static void NewWindow()
    {
        if (CurrentWindow.Count > 0)
        {
            Instructions.AddRange(CurrentWindow);
            CurrentWindow.Clear();
        }
    }

    internal static bool OptPushPop()
    {
        var cur = CurrentWindow.Pop();
        var prev = CurrentWindow.Pop();

        var (curLabel, curOpcode, curOp1, curOp2, _) = cur;
        var (_, prevOpcode, prevOp1, prevOp2, _) = prev;

        if (curOpcode == Pop && prevOpcode == Push && curOp1 == prevOp1 && curOp2 == prevOp2)
        {
            CurrentWindow.Push(new ScheduledInstruction
            {
                PreTrivia = prev.PreTrivia,
                Label = curLabel,
                Opcode = Mov,
                Operand1 = curOp2,
                Operand2 = prevOp2,
                PostTrivia = cur.PostTrivia
            });
            return true;
        }

        CurrentWindow.Push(prev);
        CurrentWindow.Push(cur);
        return false;
    }
}