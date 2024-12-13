namespace AnnaSim.TinyC;

public class CompilerContext
{
    public Scope GlobalScope { get; } = new() { Name = "<global>", Type = "void", IsGlobal = true };

    public Dictionary<string, Scope> Functions { get; internal set; } = [];

    public Dictionary<string, string> InternedStrings { get; } = [];

    public Scope CurrentScope { get; set; }

    public static bool InFuncDecl { get; set; }

    public bool TracingComments { get; set; } = false;

    public CompilerContext()
    {
        CurrentScope = GlobalScope;
    }
}