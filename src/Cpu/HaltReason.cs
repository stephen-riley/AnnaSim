namespace AnnaSim;

public enum HaltReason
{
    Running = -1,
    Halt = 0,
    Breakpoint,
    CyclesExceeded,
    DebuggerStep,
}