namespace AnnaSim;

public enum HaltReason
{
    Halt = 0,
    Breakpoint,
    CyclesExceeded,
    DebuggerStep,
}