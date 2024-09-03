using AnnaSim.TinyC.Scheduler.Instructions;

namespace AnnaSim.TinyC.Scheduler;

public static class Scheduler
{
    public static List<ScheduledInstruction> Instructions { get; } = [];
    public static Stack<ScheduledInstruction> CurrentWindow { get; } = [];

    public static void NewWindow()
    {
        if (CurrentWindow.Count > 0)
        {
            Instructions.AddRange(CurrentWindow);
            CurrentWindow.Clear();
        }
    }
}