namespace AnnaSim;

public enum HaltReason
{
    Running = -1,
    Halted = 0,
    Breakpoint,
    CyclesExceeded,
    DebuggerSingleStep,
}